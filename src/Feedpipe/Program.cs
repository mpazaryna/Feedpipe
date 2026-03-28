using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
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
