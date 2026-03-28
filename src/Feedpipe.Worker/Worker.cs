using Feedpipe.Core.Services;
using Feedpipe.Models;
using Microsoft.Extensions.Options;

namespace Feedpipe.Worker;

/// <summary>
/// A long-running background service that fetches RSS feeds on a recurring schedule.
/// </summary>
/// <remarks>
/// <para>
/// This class extends <see cref="BackgroundService"/>, which is the .NET standard
/// base class for long-running hosted services. The runtime calls
/// <see cref="ExecuteAsync"/> once at startup, and the method runs until the
/// application is shut down (via Ctrl+C, SIGTERM, or stopping the service).
/// </para>
///
/// <para><b>Primary constructor syntax (C# 12):</b></para>
/// <para>
/// The parameters in the class declaration (<c>fetcher</c>, <c>writer</c>, etc.)
/// are <b>primary constructor parameters</b>. They're injected by the DI container
/// and available throughout the class without explicit field declarations. This is
/// equivalent to writing a constructor, private fields, and assignments -- just less
/// boilerplate. You'll see both styles in .NET codebases; primary constructors are
/// newer and increasingly preferred for simple DI scenarios.
/// </para>
///
/// <para><b>IOptions&lt;T&gt; pattern:</b></para>
/// <para>
/// Instead of injecting <c>AppSettings</c> directly, we inject
/// <see cref="IOptions{TOptions}"/>. This is the .NET Options pattern -- it provides
/// a layer of indirection that supports validation, named options, and hot-reload
/// of configuration values without restarting the app.
/// </para>
///
/// <para><b>CancellationToken:</b></para>
/// <para>
/// The <c>stoppingToken</c> is signaled when the host wants to shut down. We pass
/// it to <c>Task.Delay</c> so the service stops promptly rather than waiting
/// for the full delay to elapse. Always propagate cancellation tokens in .NET async
/// code -- it's how graceful shutdown works.
/// </para>
/// </remarks>
/// <param name="fetcher">The feed fetching service, resolved from DI.</param>
/// <param name="writer">The feed writing service, resolved from DI.</param>
/// <param name="settings">Typed configuration from appsettings.json.</param>
/// <param name="logger">Typed logger for this worker.</param>
public class Worker(
    IFeedFetcher fetcher,
    IFeedWriter writer,
    IOptions<AppSettings> settings,
    ILogger<Worker> logger) : BackgroundService
{
    /// <summary>
    /// The main execution loop. Runs continuously until the application shuts down.
    /// </summary>
    /// <param name="stoppingToken">
    /// Triggered when the host is performing a graceful shutdown.
    /// </param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Feed pipeline starting");

            foreach (var feed in settings.Value.Feeds)
            {
                var items = await fetcher.FetchAsync(feed.Url);
                if (items.Count > 0)
                {
                    await writer.WriteAsync(items, feed.Name);
                }
            }

            logger.LogInformation("Feed pipeline complete. Next run in 5 minutes");
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
