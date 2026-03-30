using Conduit.Core.Models;

namespace Conduit.Core.Services;

/// <summary>
/// Extends output writing with support for transformed records —
/// both writing enriched output and reading previous output for
/// dedup lookups and queries.
/// </summary>
public interface ITransformedOutputWriter
{
    /// <summary>
    /// Persists a collection of transformed records.
    /// </summary>
    /// <param name="items">The transformed records to persist.</param>
    /// <param name="sourceType">The adapter type (e.g., "rss", "edi834").</param>
    /// <param name="sourceName">A short identifier for the source.</param>
    Task WriteAsync(List<TransformedRecord<IPipelineRecord>> items, string sourceType, string sourceName);

    /// <summary>
    /// Reads all previously stored record IDs for a given source type.
    /// Used by the dedup stage to detect duplicates across runs.
    /// </summary>
    /// <param name="sourceType">The adapter type to read IDs for.</param>
    /// <returns>A set of previously stored record IDs.</returns>
    Task<HashSet<string>> ReadPreviousIdsAsync(string sourceType);
}
