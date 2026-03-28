using System.Text.Json;
using Microsoft.Extensions.Logging;
using Feedpipe.Core.Models;
using Feedpipe.Core.Services;

namespace Feedpipe.Services;

/// <summary>
/// Persists feed items as formatted JSON files on the local filesystem.
/// </summary>
/// <remarks>
/// <para>
/// Each call to <see cref="WriteAsync"/> creates a new timestamped file
/// (e.g., <c>hacker-news_2024-01-15_143022.json</c>). This append-only
/// approach preserves history and avoids overwriting previous fetches.
/// </para>
///
/// <para><b>Why static readonly for JsonOptions?</b></para>
/// <para>
/// <see cref="JsonSerializerOptions"/> is expensive to construct because it
/// caches type metadata internally. Creating it once as a <c>static readonly</c>
/// field means all calls share the same instance. This is a common .NET
/// performance pattern -- look for it in any codebase that serializes JSON.
/// </para>
///
/// <para><b>Thread safety:</b></para>
/// <para>
/// This class is safe to register as a singleton in DI because
/// <see cref="JsonSerializer"/> is thread-safe when using a shared
/// <see cref="JsonSerializerOptions"/> instance (since .NET 8+).
/// </para>
/// </remarks>
public class JsonFeedWriter : IFeedWriter
{
    /// <summary>
    /// Shared serializer options. <c>WriteIndented = true</c> produces
    /// human-readable JSON with line breaks and indentation.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _outputDir;
    private readonly ILogger<JsonFeedWriter> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="JsonFeedWriter"/>.
    /// </summary>
    /// <param name="outputDir">
    /// The directory where JSON files will be written. Created automatically
    /// if it does not exist.
    /// </param>
    /// <param name="logger">Typed logger for this component.</param>
    public JsonFeedWriter(string outputDir, ILogger<JsonFeedWriter> logger)
    {
        _outputDir = outputDir;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// The filename format is <c>{feedName}_{yyyy-MM-dd_HHmmss}.json</c>.
    /// <c>Directory.CreateDirectory</c> is idempotent -- calling it
    /// on an existing directory is a no-op, so we don't need to check first.
    /// </remarks>
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
