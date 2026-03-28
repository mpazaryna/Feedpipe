// -----------------------------------------------------------------------
// Feedpipe Worker - Background Service Host
//
// This is the long-running version of the pipeline. Instead of running
// once and exiting (like the console app), this runs as a hosted service
// that fetches feeds every 5 minutes.
//
// KEY DIFFERENCES FROM THE CONSOLE APP:
//
// 1. Host.CreateApplicationBuilder() -- This is the .NET Generic Host,
//    which provides DI, configuration, logging, and lifecycle management
//    out of the box. The console app builds these manually; the host
//    wires them up automatically.
//
// 2. AddHostedService<Worker>() -- Registers our Worker class as a
//    background service. The host starts it automatically, manages its
//    lifecycle, and handles graceful shutdown (SIGTERM/Ctrl+C).
//
// 3. AddSerilog() (no parameters) -- In the hosted model, this extension
//    method replaces the default .NET logging providers with Serilog.
//    It uses the static Log.Logger configured above.
//
// 4. host.Run() -- Blocks until shutdown is requested. In production,
//    this would run as a systemd service, Windows Service, or container.
//
// RUN WITH: dotnet run --project src/Feedpipe.Worker
// STOP WITH: Ctrl+C (sends graceful shutdown signal)
// -----------------------------------------------------------------------

using Feedpipe.Core.Services;
using Feedpipe.Models;
using Feedpipe.Services;
using Feedpipe.Worker;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/worker-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSerilog();

// Bind the "App" section of appsettings.json to AppSettings and make it
// available as IOptions<AppSettings> throughout the DI container.
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("App"));
builder.Services.AddHttpClient<IFeedFetcher, RssFeedFetcher>();
builder.Services.AddSingleton<IFeedWriter, JsonFeedWriter>(sp =>
    new JsonFeedWriter(
        builder.Configuration.GetSection("App")["OutputDir"] ?? "fetched",
        sp.GetRequiredService<ILogger<JsonFeedWriter>>()));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
