using Conduit.Core.Models;

namespace Conduit.Core.Services;

/// <summary>
/// Chains transform stages sequentially, wrapping raw records into
/// <see cref="TransformedRecord{T}"/> envelopes and passing them through
/// each stage in order.
/// </summary>
/// <remarks>
/// <para>
/// <b>Sequential, not parallel.</b> Stages run one after the other (not concurrently)
/// because the order is load-bearing: validation must run before dedup (no point
/// deduplicating an invalid record), and dedup must run before enrichment (no point
/// enriching a duplicate). <c>foreach</c> + <c>await</c> enforces this; using
/// <c>Task.WhenAll</c> here would be a bug.
/// </para>
/// <para>
/// <b>Static factory method.</b> <see cref="CreateForSource"/> is a static method on
/// the class itself rather than a separate factory class. It lives here because it only
/// assembles <see cref="TransformPipeline"/> from Core-level types (no Transforms
/// assembly references). Compare with <c>PipelineFactory</c> in
/// <c>Conduit.Transforms</c>, which can reference validator and enrichment types.
/// </para>
/// </remarks>
public class TransformPipeline
{
    private readonly List<ITransform> _stages;

    /// <summary>
    /// Creates a new transform pipeline with the given stages.
    /// </summary>
    /// <param name="stages">Stages to execute in order.</param>
    public TransformPipeline(List<ITransform> stages)
    {
        _stages = stages;
    }

    /// <summary>
    /// Wraps raw pipeline records into envelopes and runs them through
    /// all registered stages sequentially.
    /// </summary>
    /// <param name="records">Raw records from a source adapter.</param>
    /// <returns>Transformed records after all stages have executed.</returns>
    public async Task<List<TransformedRecord<IPipelineRecord>>> ExecuteAsync(
        List<IPipelineRecord> records)
    {
        // LINQ projection: `.Select()` maps each raw record to a TransformedRecord envelope.
        // `.ToList()` materializes the lazy sequence immediately — required because we need
        // a concrete List<> to pass into the first stage, not a deferred IEnumerable<>.
        var transformed = records
            .Select(r => new TransformedRecord<IPipelineRecord>(r))
            .ToList();

        // Each stage receives the output of the previous stage.
        // `await` suspends this method until the stage completes before moving to the next.
        foreach (var stage in _stages)
        {
            transformed = await stage.ExecuteAsync(transformed);
        }

        return transformed;
    }

    /// <summary>
    /// Creates a pipeline with cross-run dedup for the given source type.
    /// </summary>
    /// <param name="writer">The transformed output writer for reading previous IDs.</param>
    /// <param name="sourceType">The source type being processed.</param>
    /// <param name="enrichmentTransforms">Additional enrichment transforms to include.</param>
    /// <returns>A pipeline configured for the given source type.</returns>
    public static TransformPipeline CreateForSource(
        ITransformedOutputWriter writer,
        string sourceType,
        IEnumerable<ITransform> enrichmentTransforms)
    {
        var stages = new List<ITransform>
        {
            new DeduplicationTransform(writer, sourceType)
        };
        stages.AddRange(enrichmentTransforms);
        return new TransformPipeline(stages);
    }
}
