using Feedpipe.Core.Models;
using Feedpipe.Core.Services;
using Feedpipe.Models;
using Feedpipe.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSerilog();
builder.Services.AddOpenApi();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("App"));
builder.Services.AddHttpClient<IFeedFetcher, RssFeedFetcher>();
builder.Services.AddSingleton<IFeedWriter, JsonFeedWriter>(sp =>
    new JsonFeedWriter(
        builder.Configuration.GetSection("App")["OutputDir"] ?? "fetched",
        sp.GetRequiredService<ILogger<JsonFeedWriter>>()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// GET /feeds - list configured feeds
app.MapGet("/feeds", (Microsoft.Extensions.Options.IOptions<AppSettings> settings) =>
    settings.Value.Feeds)
    .WithName("GetFeeds");

// GET /feeds/{name}/fetch - fetch a feed live and return items
app.MapGet("/feeds/{name}/fetch", async (string name, IFeedFetcher fetcher,
    Microsoft.Extensions.Options.IOptions<AppSettings> settings) =>
{
    var feed = settings.Value.Feeds.FirstOrDefault(f =>
        string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));

    if (feed is null)
        return Results.NotFound(new { error = $"Feed '{name}' not found" });

    var items = await fetcher.FetchAsync(feed.Url);
    return Results.Ok(items);
})
.WithName("FetchFeed");

// GET /feeds/{name}/items - read previously fetched items from disk
app.MapGet("/feeds/{name}/items", (string name,
    Microsoft.Extensions.Options.IOptions<AppSettings> settings) =>
{
    var outputDir = settings.Value.OutputDir;
    if (!Directory.Exists(outputDir))
        return Results.Ok(Array.Empty<FeedItem>());

    var files = Directory.GetFiles(outputDir, $"{name}_*.json")
        .OrderByDescending(f => f)
        .FirstOrDefault();

    if (files is null)
        return Results.NotFound(new { error = $"No fetched data for '{name}'" });

    var json = File.ReadAllText(files);
    var items = System.Text.Json.JsonSerializer.Deserialize<List<FeedItem>>(json);
    return Results.Ok(items);
})
.WithName("GetFeedItems");

// POST /feeds/{name}/fetch - fetch and save to disk
app.MapPost("/feeds/{name}/fetch", async (string name, IFeedFetcher fetcher,
    IFeedWriter writer, Microsoft.Extensions.Options.IOptions<AppSettings> settings) =>
{
    var feed = settings.Value.Feeds.FirstOrDefault(f =>
        string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));

    if (feed is null)
        return Results.NotFound(new { error = $"Feed '{name}' not found" });

    var items = await fetcher.FetchAsync(feed.Url);
    if (items.Count > 0)
    {
        await writer.WriteAsync(items, feed.Name);
    }

    return Results.Ok(new { feed = feed.Name, itemCount = items.Count });
})
.WithName("FetchAndSaveFeed");

app.Run();
