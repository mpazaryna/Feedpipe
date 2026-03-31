using Conduit.Core.Services;
using Conduit.Models;
using Conduit.Transforms;
using Microsoft.Extensions.Options;

namespace Conduit.Worker;

/// <summary>
/// A long-running background service that ingests data sources on a recurring schedule.
/// </summary>
/// <remarks>
/// <para>
/// Extends <see cref="BackgroundService"/>, the .NET standard base class for
/// long-running hosted services. The runtime calls <see cref="ExecuteAsync"/>
/// once at startup, and it runs until the application shuts down.
/// </para>
///
/// <para><b>Adapter routing:</b></para>
/// <para>
/// Each source's <c>Type</c> field is used as a key to resolve the correct
/// <see cref="ISourceAdapter"/> from the DI container using keyed services.
/// This allows the worker to process heterogeneous sources (RSS, EDI 834, etc.)
/// without knowing the concrete adapter types.
/// </para>
///
/// <para><b>CancellationToken:</b></para>
/// <para>
/// The <c>stoppingToken</c> is signaled on shutdown. We pass it to
/// <c>Task.Delay</c> so the service stops promptly. Always propagate
/// cancellation tokens in .NET async code.
/// </para>
/// </remarks>
/// <param name="serviceProvider">The DI service provider for resolving keyed adapters.</param>
/// <param name="writer">The output writer for raw data, resolved from DI.</param>
/// <param name="transformedWriter">The output writer for curated data.</param>
/// <param name="rejectedWriter">The output writer for rejected (invalid) records.</param>
/// <param name="enrichmentTransforms">Registered enrichment transforms.</param>
/// <param name="validators">Registered record validators.</param>
/// <param name="settings">Typed configuration from appsettings.json.</param>
/// <param name="logger">Typed logger for this worker.</param>
public class Worker(
    IServiceProvider serviceProvider,
    IOutputWriter writer,
    ITransformedOutputWriter transformedWriter,
    IRejectedOutputWriter rejectedWriter,
    IReadOnlyList<ITransform> enrichmentTransforms,
    IReadOnlyList<IRecordValidator> validators,
    IOptions<AppSettings> settings,
    ILogger<Worker> logger) : BackgroundService
{
    /// <summary>
    /// The main execution loop. Runs continuously until the application shuts down.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Pipeline starting");

            var semaphore = new SemaphoreSlim(4);
            var tasks = settings.Value.Sources.Select(async source =>
            {
                await semaphore.WaitAsync(stoppingToken);
                try
                {
                    var adapter = serviceProvider.GetRequiredKeyedService<ISourceAdapter>(source.Type);
                    var items = await adapter.IngestAsync(source.Location);
                    if (items.Count > 0)
                    {
                        if (source.Type == "edi834" && File.Exists(source.Location))
                        {
                            var rawDir = Path.Combine(settings.Value.OutputDir, source.Type);
                            Directory.CreateDirectory(rawDir);
                            var rawPath = Path.Combine(rawDir, $"{source.Name}_{DateTime.Now:yyyy-MM-dd_HHmmss}.edi");
                            File.Copy(source.Location, rawPath);
                            logger.LogInformation("Copied raw EDI file to {Path}", rawPath);
                        }
                        else
                        {
                            await writer.WriteAsync(items, source.Type, source.Name);
                        }

                        var pipeline = PipelineFactory.CreateForSource(
                            transformedWriter, rejectedWriter, source.Type, source.Name, validators, enrichmentTransforms);
                        var transformed = await pipeline.ExecuteAsync(items);
                        if (transformed.Count > 0)
                        {
                            await transformedWriter.WriteAsync(transformed, source.Type, source.Name);
                        }
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            logger.LogInformation("Pipeline complete. Next run in 5 minutes");
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
