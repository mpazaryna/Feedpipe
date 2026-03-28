using System.Text.Json;
using Microsoft.Extensions.Logging;
using Feedpipe.Models;

namespace Feedpipe.Services;

public class JsonFeedWriter : IFeedWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _outputDir;
    private readonly ILogger<JsonFeedWriter> _logger;

    public JsonFeedWriter(string outputDir, ILogger<JsonFeedWriter> logger)
    {
        _outputDir = outputDir;
        _logger = logger;
    }

    public async Task WriteAsync(List<FeedItem> items, string feedName)
    {
        Directory.CreateDirectory(_outputDir);

        var filename = $"{feedName}_{DateTime.Now:yyyy-MM-dd_HHmmss}.json";
        var path = Path.Combine(_outputDir, filename);

        var json = JsonSerializer.Serialize(items, JsonOptions);
        await File.WriteAllTextAsync(path, json);

        _logger.LogInformation("Wrote {Count} items to {Path}", items.Count, path);
    }
}
