using Conduit.Core.Models;

namespace Conduit.Core.Services;

/// <summary>
/// Transform stage that filters out duplicate records — both within the
/// current batch and against previously stored transformed output.
/// </summary>
/// <remarks>
/// <para>
/// Uses <see cref="IPipelineRecord.Id"/> as the default dedup key.
/// Records implementing <see cref="ICompositeDedupKey"/> provide a
/// custom key for source types where uniqueness requires multiple fields
/// (e.g., EDI 834: subscriber ID + coverage start date + plan ID).
/// </para>
/// <para>
/// When constructed with an <see cref="ITransformedOutputWriter"/> and source type,
/// the stage loads previously stored record IDs before deduplicating, enabling
/// cross-run dedup (e.g., the same RSS article won't be stored again on the next run).
/// </para>
/// </remarks>
public class DeduplicationTransform : ITransform
{
    private readonly ITransformedOutputWriter? _writer;
    private readonly string? _sourceType;

    /// <summary>
    /// Creates a dedup stage that only deduplicates within the current batch.
    /// </summary>
    public DeduplicationTransform()
    {
    }

    /// <summary>
    /// Creates a dedup stage that deduplicates against both the current batch
    /// and previously stored transformed output.
    /// </summary>
    /// <param name="writer">The transformed output writer to read previous IDs from.</param>
    /// <param name="sourceType">The source type to load previous IDs for.</param>
    public DeduplicationTransform(ITransformedOutputWriter writer, string sourceType)
    {
        _writer = writer;
        _sourceType = sourceType;
    }

    /// <inheritdoc />
    public async Task<List<TransformedRecord<IPipelineRecord>>> ExecuteAsync(
        List<TransformedRecord<IPipelineRecord>> records)
    {
        // If a writer was provided, seed `seen` with IDs already written to disk.
        // This enables cross-run dedup: articles ingested on a previous run won't
        // be written again on the next run.
        // `is not null` is the idiomatic null check in modern C# (preferred over != null).
        var seen = _writer is not null && _sourceType is not null
            ? await _writer.ReadPreviousIdsAsync(_sourceType)
            : new HashSet<string>();

        var result = new List<TransformedRecord<IPipelineRecord>>();

        foreach (var record in records)
        {
            // Pattern matching with `is` — checks the type AND binds to a variable in one step.
            // If the record implements ICompositeDedupKey, `composite` is the typed reference.
            // The ternary `?:` selects the composite key or falls back to the simple Id.
            var key = record.Record is ICompositeDedupKey composite
                ? composite.DedupKey
                : record.Record.Id;

            // HashSet<T>.Add() returns true if the item was newly added, false if it already existed.
            // This is the idiomatic C# way to "add and check in one step" — no separate Contains() call.
            if (seen.Add(key))
            {
                result.Add(record);
            }
        }

        return result;
    }
}
