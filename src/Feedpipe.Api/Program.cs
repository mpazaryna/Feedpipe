// -----------------------------------------------------------------------
// Feedpipe API - REST Endpoints for Feed Data
//
// This is an ASP.NET Minimal API that exposes the feed pipeline over HTTP.
// Minimal APIs are the modern .NET approach for building lightweight HTTP
// services -- they use lambda expressions instead of controller classes.
//
// ENDPOINTS:
//   GET  /feeds                  -- List all configured feeds
//   GET  /feeds/{name}/fetch     -- Fetch a feed live and return items
//   GET  /feeds/{name}/items     -- Read previously fetched items from disk
//   POST /feeds/{name}/fetch     -- Fetch a feed and save results to disk
//
// KEY CONCEPTS:
//
// 1. WebApplication.CreateBuilder() -- Similar to the Worker's host builder,
//    but adds HTTP server capabilities (Kestrel), routing, and middleware.
//
// 2. Minimal API routing -- app.MapGet("/path", handler) maps a URL pattern
//    to a lambda. Parameters are automatically bound from the route, query
//    string, or DI container based on their type.
//
// 3. Results.Ok() / Results.NotFound() -- These are "typed results" that
//    set the HTTP status code and serialize the response body as JSON.
//
// 4. Parameter injection in handlers -- ASP.NET inspects the lambda's
//    parameters and resolves them: route values (string name), DI services
//    (IFeedFetcher), and options (IOptions<AppSettings>).
//
// 5. OpenAPI -- app.MapOpenApi() serves the API specification at
//    /openapi/v1.json in development mode. Tools like Swagger UI can
//    consume this to generate interactive API documentation.
//
// RUN WITH: dotnet run --project src/Feedpipe.Api
// TEST WITH: curl http://localhost:5000/feeds
// -----------------------------------------------------------------------

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

// GET /feeds -- Returns the list of configured feed sources from appsettings.json.
// The IOptions<AppSettings> parameter is resolved from DI automatically.
app.MapGet("/feeds", (Microsoft.Extensions.Options.IOptions<AppSettings> settings) =>
    settings.Value.Feeds)
    .WithName("GetFeeds");

// GET /feeds/{name}/fetch -- Fetches a feed live over HTTP and returns the parsed
// items directly without saving to disk. Useful for previewing feed content.
// The {name} route parameter is matched case-insensitively against configured feeds.
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

// GET /feeds/{name}/items -- Reads the most recently fetched JSON file from disk
// for the given feed. Returns the stored items without making any network requests.
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

// POST /feeds/{name}/fetch -- Fetches a feed and persists the results to disk.
// Returns a summary with the feed name and item count. This is the endpoint
// you would call from a scheduler or webhook to trigger a pipeline run.
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
