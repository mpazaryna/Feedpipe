// -----------------------------------------------------------------------
// Conduit API - REST Endpoints for Pipeline Data
//
// ASP.NET Minimal API that exposes the pipeline over HTTP.
//
// ENDPOINTS:
//   GET  /sources                  -- List all configured sources
//   GET  /sources/{name}/ingest    -- Ingest a source live and return items
//   GET  /sources/{name}/items     -- Read previously ingested items from disk
//   POST /sources/{name}/ingest    -- Ingest a source and save results to disk
//
// RUN WITH: dotnet run --project src/Conduit.Api
// TEST WITH: curl http://localhost:5000/sources
// -----------------------------------------------------------------------

using Conduit.Core.Services;
using Conduit.Models;
using Conduit.Services;
using Conduit.Transforms;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSerilog();
builder.Services.AddOpenApi();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("App"));
builder.Services.AddHttpClient();

var appSection = builder.Configuration.GetSection("App");
builder.Services.AddConduitPipeline(
    appSection["OutputDir"] ?? "data/raw",
    appSection["CuratedOutputDir"] ?? "data/curated",
    appSection["RejectedOutputDir"] ?? "data/rejected");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// GET /health -- Health probe for Azure App Service / Container Apps.
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("Health");

// GET /sources -- Returns the list of configured sources from appsettings.json.
app.MapGet("/sources", (Microsoft.Extensions.Options.IOptions<AppSettings> settings) =>
    settings.Value.Sources)
    .WithName("GetSources");

// GET /sources/{name}/ingest -- Ingests a source live and returns the parsed items.
app.MapGet("/sources/{name}/ingest", async (string name, IServiceProvider sp,
    Microsoft.Extensions.Options.IOptions<AppSettings> settings) =>
{
    var source = settings.Value.Sources.FirstOrDefault(s =>
        string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    if (source is null)
        return Results.NotFound(new { error = $"Source '{name}' not found" });

    var adapter = sp.GetRequiredKeyedService<ISourceAdapter>(source.Type);
    var items = await adapter.IngestAsync(source.Location);
    return Results.Ok(items);
})
.WithName("IngestSource");

// GET /sources/{name}/items -- Reads the most recently transformed items from disk.
app.MapGet("/sources/{name}/items", (string name,
    Microsoft.Extensions.Options.IOptions<AppSettings> settings) =>
{
    var source = settings.Value.Sources.FirstOrDefault(s =>
        string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    if (source is null)
        return Results.NotFound(new { error = $"Source '{name}' not found" });

    var transformedDir = Path.Combine(settings.Value.CuratedOutputDir, source.Type);
    if (!Directory.Exists(transformedDir))
        return Results.Ok(Array.Empty<object>());

    var latestFile = Directory.GetFiles(transformedDir, $"{name}_*.json")
        .OrderByDescending(f => f)
        .FirstOrDefault();

    if (latestFile is null)
        return Results.NotFound(new { error = $"No transformed data for '{name}'" });

    var json = File.ReadAllText(latestFile);
    var items = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
    return Results.Ok(items);
})
.WithName("GetSourceItems");

// POST /sources/{name}/ingest -- Ingests a source, transforms, and persists results to disk.
app.MapPost("/sources/{name}/ingest", async (string name, IServiceProvider sp,
    IOutputWriter writer, ITransformedOutputWriter transformedWriter,
    IRejectedOutputWriter rejectedWriter,
    Microsoft.Extensions.Options.IOptions<AppSettings> settings) =>
{
    var source = settings.Value.Sources.FirstOrDefault(s =>
        string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    if (source is null)
        return Results.NotFound(new { error = $"Source '{name}' not found" });

    var adapter = sp.GetRequiredKeyedService<ISourceAdapter>(source.Type);
    var items = await adapter.IngestAsync(source.Location);
    if (items.Count > 0)
    {
        if (source.Type == "edi834" && File.Exists(source.Location))
        {
            var rawDir = Path.Combine(settings.Value.OutputDir, source.Type);
            Directory.CreateDirectory(rawDir);
            var rawPath = Path.Combine(rawDir, $"{source.Name}_{DateTime.Now:yyyy-MM-dd_HHmmss}.edi");
            File.Copy(source.Location, rawPath);
        }
        else
        {
            await writer.WriteAsync(items, source.Type, source.Name);
        }

        var enrichmentTransforms = sp.GetRequiredService<IReadOnlyList<ITransform>>();
        var validators = sp.GetRequiredService<IReadOnlyList<IRecordValidator>>();
        var pipeline = PipelineFactory.CreateForSource(
            transformedWriter, rejectedWriter, source.Type, source.Name, validators, enrichmentTransforms);
        var transformed = await pipeline.ExecuteAsync(items);
        if (transformed.Count > 0)
        {
            await transformedWriter.WriteAsync(transformed, source.Type, source.Name);
        }
    }

    return Results.Ok(new { source = source.Name, itemCount = items.Count });
})
.WithName("IngestAndSaveSource");

app.Run();

public partial class Program { }
