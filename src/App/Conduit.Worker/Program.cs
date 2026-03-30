// -----------------------------------------------------------------------
// Conduit Worker - Background Service Host
//
// Long-running version of the pipeline. Ingests sources every 5 minutes.
//
// RUN WITH: dotnet run --project src/Conduit.Worker
// STOP WITH: Ctrl+C (sends graceful shutdown signal)
// -----------------------------------------------------------------------

using Conduit.Models;
using Conduit.Services;
using Conduit.Worker;
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

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("App"));
builder.Services.AddHttpClient();

var appSection = builder.Configuration.GetSection("App");
builder.Services.AddConduitPipeline(
    appSection["OutputDir"] ?? "data/raw",
    appSection["CuratedOutputDir"] ?? "data/curated");

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
