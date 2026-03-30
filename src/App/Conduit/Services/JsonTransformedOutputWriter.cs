using System.Text.Json;
using Microsoft.Extensions.Logging;
using Conduit.Core.Models;
using Conduit.Core.Services;

namespace Conduit.Services;

/// <summary>
/// Persists transformed pipeline records as JSON files and reads
/// previous output for deduplication lookups.
/// </summary>
/// <remarks>
/// Output is organized as <c>{outputDir}/{sourceType}/{sourceName}_{timestamp}.json</c>.
/// Each file contains an array of envelope objects with "record" and "enrichment" fields.
/// </remarks>
public class JsonTransformedOutputWriter : ITransformedOutputWriter
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _outputDir;
    private readonly ILogger<JsonTransformedOutputWriter> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="JsonTransformedOutputWriter"/>.
    /// </summary>
    /// <param name="outputDir">Root directory for transformed output.</param>
    /// <param name="logger">Typed logger.</param>
    public JsonTransformedOutputWriter(string outputDir, ILogger<JsonTransformedOutputWriter> logger)
    {
        _outputDir = outputDir;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task WriteAsync(List<TransformedRecord<IPipelineRecord>> items, string sourceType, string sourceName)
    {
        var typeDir = Path.Combine(_outputDir, sourceType);
        Directory.CreateDirectory(typeDir);

        var filename = $"{sourceName}_{DateTime.Now:yyyy-MM-dd_HHmmss}.json";
        var path = Path.Combine(typeDir, filename);

        var envelopes = items.Select(item => new
        {
            record = (object)item.Record,
            enrichment = item.Enrichment
        }).ToList();

        var json = JsonSerializer.Serialize(envelopes, WriteOptions);
        await File.WriteAllTextAsync(path, json);

        _logger.LogInformation("Wrote {Count} transformed items to {Path}", items.Count, path);
    }

    /// <inheritdoc />
    public async Task<HashSet<string>> ReadPreviousIdsAsync(string sourceType)
    {
        var typeDir = Path.Combine(_outputDir, sourceType);
        var ids = new HashSet<string>();

        if (!Directory.Exists(typeDir))
        {
            return ids;
        }

        foreach (var file in Directory.GetFiles(typeDir, "*.json"))
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                using var doc = JsonDocument.Parse(json);

                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.TryGetProperty("record", out var record) &&
                        record.TryGetProperty("id", out var id))
                    {
                        ids.Add(id.GetString()!);
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse {File} for dedup IDs", file);
            }
        }

        return ids;
    }
}
