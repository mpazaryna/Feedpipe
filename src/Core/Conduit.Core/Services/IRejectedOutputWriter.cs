using Conduit.Core.Models;

namespace Conduit.Core.Services;

/// <summary>
/// Defines the contract for persisting records that failed validation.
/// </summary>
/// <remarks>
/// Rejected records are written to a separate output tier (e.g., <c>data/rejected/</c>)
/// alongside the raw and curated tiers. The original record is preserved with the
/// validation errors that caused it to be rejected.
/// </remarks>
public interface IRejectedOutputWriter
{
    /// <summary>
    /// Persists a collection of rejected records organized by source type and name.
    /// </summary>
    /// <param name="items">The rejected records to persist.</param>
    /// <param name="sourceType">
    /// The adapter type (e.g., "rss", "edi834"). Used to organize output
    /// into subdirectories by source type.
    /// </param>
    /// <param name="sourceName">
    /// A short identifier for the source (e.g., "benefits-enrollment"). Used in
    /// the output filename.
    /// </param>
    Task WriteAsync(List<RejectedRecord<IPipelineRecord>> items, string sourceType, string sourceName);
}
