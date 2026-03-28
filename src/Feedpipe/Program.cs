// -----------------------------------------------------------------------
// Feedpipe Console Runner
//
// This is the simplest way to run the pipeline: a one-shot console app
// that fetches all configured feeds, writes the results to disk, and exits.
//
// ARCHITECTURE NOTES:
//
// 1. Configuration -- We load settings from appsettings.json using the
//    .NET Configuration system. AppContext.BaseDirectory ensures the file
//    is found relative to the compiled output, not the working directory.
//
// 2. Logging -- Serilog is configured with two "sinks": Console (for
//    immediate feedback) and File (for audit/debugging). The file sink
//    uses daily rolling, creating a new log file each day.
//
// 3. Dependency Injection -- Even in a console app, we use a DI container
//    (ServiceCollection) to wire up dependencies. This ensures our services
//    are created the same way they would be in the Worker or Api projects.
//
//    Key registrations:
//    - AddHttpClient<IFeedFetcher, RssFeedFetcher>() -- registers both
//      the HttpClient (via IHttpClientFactory) and the fetcher service.
//    - AddSingleton<IFeedWriter>() -- single instance for the app lifetime.
//
// 4. Pipeline Loop -- Iterates over each configured feed, fetches items,
//    and writes non-empty results to disk. Errors in one feed don't stop
//    the others (handled inside RssFeedFetcher).
//
// RUN WITH: dotnet run --project src/Feedpipe
// -----------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Feedpipe.Core.Services;
using Feedpipe.Models;
using Feedpipe.Services;

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
services.AddHttpClient<IFeedFetcher, RssFeedFetcher>();
services.AddSingleton<IFeedWriter>(sp =>
    new JsonFeedWriter(appSettings.OutputDir, sp.GetRequiredService<ILogger<JsonFeedWriter>>()));

var provider = services.BuildServiceProvider();

// -- Run the pipeline --
var logger = provider.GetRequiredService<ILogger<Program>>();
var fetcher = provider.GetRequiredService<IFeedFetcher>();
var writer = provider.GetRequiredService<IFeedWriter>();

logger.LogInformation("Starting feed pipeline");

foreach (var feed in appSettings.Feeds)
{
    var items = await fetcher.FetchAsync(feed.Url);
    if (items.Count > 0)
    {
        await writer.WriteAsync(items, feed.Name);
    }
}

logger.LogInformation("Pipeline complete");
Log.CloseAndFlush();
