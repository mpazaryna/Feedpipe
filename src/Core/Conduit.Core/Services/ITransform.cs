using Conduit.Core.Models;

namespace Conduit.Core.Services;

/// <summary>
/// Defines a single transform in the transformation pipeline.
/// </summary>
/// <remarks>
/// <para>
/// <b>Chain of Responsibility pattern.</b> Each transform receives a list of records,
/// does its work, and passes the result to the next stage. Stages are composable:
/// adding a new transform means implementing this interface and registering it in DI —
/// no changes to <see cref="TransformPipeline"/> or other transforms are required.
/// Stages can filter records (deduplication), add enrichment metadata (keyword
/// extraction, status derivation), or both.
/// </para>
/// <para>
/// <b>Why <c>async Task&lt;&gt;</c>?</b> Some transforms do I/O — the deduplication
/// stage reads previously stored record IDs from disk; enrichment stages could call
/// external APIs. Making every stage async (even purely CPU-bound ones) keeps the
/// interface consistent and lets the pipeline <c>await</c> each stage uniformly.
/// A synchronous stage just returns <c>Task.FromResult(...)</c>.
/// </para>
/// <para>
/// <b>Why <c>List&lt;&gt;</c> not <c>IEnumerable&lt;&gt;</c>?</b> Transforms need to
/// iterate records multiple times (once to classify, once to build results). An
/// <c>IEnumerable</c> can only be enumerated once before exhausting its enumerator.
/// Using <c>List</c> as both input and output makes the contract explicit and avoids
/// accidental double-enumeration bugs.
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
