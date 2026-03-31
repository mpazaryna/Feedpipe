using Conduit.Core.Services;

namespace Conduit.Transforms;

/// <summary>
/// Builds configured <see cref="TransformPipeline"/> instances for a given source.
/// Lives in <c>Conduit.Transforms</c> rather than <c>Conduit.Core</c> so it can
/// reference both Core pipeline infrastructure and transform implementations
/// (validators, enrichment stages) without creating a circular dependency.
/// </summary>
public static class PipelineFactory
{
    /// <summary>
    /// Creates a pipeline with validation, cross-run dedup, and enrichment for the given source.
    /// Stage order: Validation → Deduplication → Enrichment.
    /// </summary>
    /// <param name="writer">Transformed output writer for reading previous IDs (dedup).</param>
    /// <param name="rejectedWriter">Rejected output writer for invalid records.</param>
    /// <param name="sourceType">The source type (e.g., "rss", "edi834").</param>
    /// <param name="sourceName">The source name used in output filenames.</param>
    /// <param name="validators">Record validators to apply before dedup and enrichment.</param>
    /// <param name="enrichmentTransforms">Enrichment transforms to run after dedup.</param>
    public static TransformPipeline CreateForSource(
        ITransformedOutputWriter writer,
        IRejectedOutputWriter rejectedWriter,
        string sourceType,
        string sourceName,
        IEnumerable<IRecordValidator> validators,
        IEnumerable<ITransform> enrichmentTransforms)
    {
        var stages = new List<ITransform>
        {
            new ValidationTransform(rejectedWriter, sourceType, sourceName, validators),
            new DeduplicationTransform(writer, sourceType)
        };
        stages.AddRange(enrichmentTransforms);
        return new TransformPipeline(stages);
    }
}
