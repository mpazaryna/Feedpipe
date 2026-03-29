// -----------------------------------------------------------------------
// Conduit Console Runner
//
// One-shot console app that ingests all configured sources, writes the
// results to disk, and exits.
//
// ARCHITECTURE NOTES:
//
// 1. Configuration -- Loaded from appsettings.json. AppContext.BaseDirectory
//    ensures the file is found relative to the compiled output.
//
// 2. Logging -- Serilog with Console + File sinks. Daily rolling log files.
//
// 3. Dependency Injection -- ServiceCollection wires up source adapters as
//    keyed services, resolved by SourceSettings.Type at runtime.
//
// 4. Pipeline Loop -- Iterates over each configured source, resolves the
//    correct adapter by key, ingests data, and writes non-empty results
//    to disk. Sources are processed concurrently with a semaphore.
//
// RUN WITH: dotnet run --project src/Conduit
// -----------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Conduit.Core.Services;
using Conduit.Models;
using Conduit.Services;
using Conduit.Sources.Rss.Services;
using Conduit.Sources.Edi834.Services;

// -- Load configuration from appsettings.json --
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var appSettings = configuration.GetSection("App").Get<AppSettings>()
    ?? throw new InvalidOperationException("Missing 'App' section in appsettings.json");

// -- Configure Serilog --
Directory.CreateDirectory(appSettings.LogsDir);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        Path.Combine(appSettings.LogsDir, "log-.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// -- Register services in DI container --
var services = new ServiceCollection();

services.AddLogging(builder => builder.AddSerilog(Log.Logger));
services.AddHttpClient();

// Register adapters as keyed services, resolved by SourceSettings.Type
services.AddKeyedScoped<ISourceAdapter>("rss", (sp, _) =>
    new FeedSourceAdapter(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
        sp.GetRequiredService<ILogger<FeedSourceAdapter>>()));
services.AddKeyedScoped<ISourceAdapter, Edi834SourceAdapter>("edi834");

services.AddSingleton<IOutputWriter>(sp =>
    new JsonOutputWriter(appSettings.OutputDir, sp.GetRequiredService<ILogger<JsonOutputWriter>>()));

var provider = services.BuildServiceProvider();

// -- Run the pipeline --
var logger = provider.GetRequiredService<ILogger<Program>>();
var writer = provider.GetRequiredService<IOutputWriter>();

logger.LogInformation("Starting pipeline");

// Process sources concurrently with a semaphore to limit parallelism
var semaphore = new SemaphoreSlim(4);
var tasks = appSettings.Sources.Select(async source =>
{
    await semaphore.WaitAsync();
    try
    {
        var adapter = provider.GetRequiredKeyedService<ISourceAdapter>(source.Type);
        var items = await adapter.IngestAsync(source.Location);
        if (items.Count > 0)
        {
            await writer.WriteAsync(items, source.Type, source.Name);
        }
    }
    finally
    {
        semaphore.Release();
    }
});

await Task.WhenAll(tasks);

logger.LogInformation("Pipeline complete");
Log.CloseAndFlush();
