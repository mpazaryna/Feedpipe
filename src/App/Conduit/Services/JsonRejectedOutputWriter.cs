using System.Text.Json;
using Microsoft.Extensions.Logging;
using Conduit.Core.Models;
using Conduit.Core.Services;

namespace Conduit.Services;

/// <summary>
/// Persists rejected pipeline records as formatted JSON files on the local filesystem,
/// organized by source type under the rejected output directory.
/// </summary>
/// <remarks>
/// Output is organized as <c>{outputDir}/{sourceType}/{sourceName}_{timestamp}.json</c>.
/// Each entry contains the original record, human-readable validation errors, and
/// the timestamp when the record was rejected.
/// </remarks>
public class JsonRejectedOutputWriter : IRejectedOutputWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _outputDir;
    private readonly ILogger<JsonRejectedOutputWriter> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="JsonRejectedOutputWriter"/>.
    /// </summary>
    /// <param name="outputDir">Root directory for rejected output (e.g., "data/rejected").</param>
    /// <param name="logger">Typed logger for this component.</param>
    public JsonRejectedOutputWriter(string outputDir, ILogger<JsonRejectedOutputWriter> logger)
    {
        _outputDir = outputDir;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task WriteAsync(List<RejectedRecord<IPipelineRecord>> items, string sourceType, string sourceName)
    {
        var typeDir = Path.Combine(_outputDir, sourceType);
        Directory.CreateDirectory(typeDir);

        var filename = $"{sourceName}_{DateTime.Now:yyyy-MM-dd_HHmmss}.json";
        var path = Path.Combine(typeDir, filename);

        var envelopes = items.Select(item => new
        {
            record = (object)item.Record,
            errors = item.Errors,
            rejectedAt = item.RejectedAt
        }).ToList();

        var json = JsonSerializer.Serialize(envelopes, JsonOptions);
        await File.WriteAllTextAsync(path, json);

        _logger.LogInformation("Wrote {Count} rejected records to {Path}", items.Count, path);
    }
}
