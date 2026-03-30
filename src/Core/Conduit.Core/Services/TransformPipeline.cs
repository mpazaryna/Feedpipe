using Conduit.Core.Models;

namespace Conduit.Core.Services;

/// <summary>
/// Chains transform stages sequentially, wrapping raw records into
/// <see cref="TransformedRecord{T}"/> envelopes and passing them through
/// each stage in order.
/// </summary>
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
        var transformed = records
            .Select(r => new TransformedRecord<IPipelineRecord>(r))
            .ToList();

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
