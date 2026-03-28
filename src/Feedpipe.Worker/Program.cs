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

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("App"));
builder.Services.AddHttpClient<IFeedFetcher, RssFeedFetcher>();
builder.Services.AddSingleton<IFeedWriter, JsonFeedWriter>(sp =>
    new JsonFeedWriter(
        builder.Configuration.GetSection("App")["OutputDir"] ?? "fetched",
        sp.GetRequiredService<ILogger<JsonFeedWriter>>()));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
