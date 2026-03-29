// -----------------------------------------------------------------------
// Conduit Worker - Background Service Host
//
// Long-running version of the pipeline. Ingests sources every 5 minutes.
//
// RUN WITH: dotnet run --project src/Conduit.Worker
// STOP WITH: Ctrl+C (sends graceful shutdown signal)
// -----------------------------------------------------------------------

using Conduit.Core.Services;
using Conduit.Models;
using Conduit.Services;
using Conduit.Sources.Rss.Services;
using Conduit.Sources.Edi834.Services;
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

// Register adapters as keyed services
builder.Services.AddKeyedScoped<ISourceAdapter>("rss", (sp, _) =>
    new FeedSourceAdapter(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
        sp.GetRequiredService<ILogger<FeedSourceAdapter>>()));
builder.Services.AddKeyedScoped<ISourceAdapter, Edi834SourceAdapter>("edi834");

builder.Services.AddSingleton<IOutputWriter, JsonOutputWriter>(sp =>
    new JsonOutputWriter(
        builder.Configuration.GetSection("App")["OutputDir"] ?? "data",
        sp.GetRequiredService<ILogger<JsonOutputWriter>>()));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
