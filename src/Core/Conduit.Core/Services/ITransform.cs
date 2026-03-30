using Conduit.Core.Models;

namespace Conduit.Core.Services;

/// <summary>
/// Defines a single transform in the transformation pipeline.
/// </summary>
/// <remarks>
/// <para>
/// Each transform receives a list of transformed records and returns a
/// (potentially filtered or enriched) list. Stages are composable —
/// adding a new transform means implementing this interface and registering
/// it in DI. No changes to the pipeline or other transforms are required.
/// </para>
/// <para>
/// Stages can filter records (deduplication), add enrichment metadata
/// (keyword extraction, status derivation), or both.
/// </para>
/// </remarks>
public interface ITransform
{
    /// <summary>
    /// Processes a list of transformed records and returns the result.
    /// </summary>
    /// <param name="records">The records to process.</param>
    /// <returns>The processed records — may be filtered, enriched, or both.</returns>
    Task<List<TransformedRecord<IPipelineRecord>>> ExecuteAsync(
        List<TransformedRecord<IPipelineRecord>> records);
}
