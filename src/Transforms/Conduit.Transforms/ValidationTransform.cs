using Conduit.Core.Models;
using Conduit.Core.Services;

namespace Conduit.Transforms;

/// <summary>
/// Transform stage that validates records against source-specific rules before
/// they reach deduplication and enrichment. Invalid records are routed to the
/// rejected output tier; only valid records flow forward.
/// </summary>
public class ValidationTransform : ITransform
{
    private readonly IRejectedOutputWriter _rejectedWriter;
    private readonly string _sourceType;
    private readonly string _sourceName;
    private readonly IEnumerable<IRecordValidator> _validators;

    /// <summary>
    /// Creates a new validation transform.
    /// </summary>
    /// <param name="rejectedWriter">Writer for invalid records.</param>
    /// <param name="sourceType">The source type being processed (e.g., "edi834").</param>
    /// <param name="sourceName">The source name for output file naming (e.g., "benefits-enrollment").</param>
    /// <param name="validators">Validators to apply; each declares which record type it handles.</param>
    public ValidationTransform(
        IRejectedOutputWriter rejectedWriter,
        string sourceType,
        string sourceName,
        IEnumerable<IRecordValidator> validators)
    {
        _rejectedWriter = rejectedWriter;
        _sourceType = sourceType;
        _sourceName = sourceName;
        _validators = validators;
    }

    /// <inheritdoc />
    public async Task<List<TransformedRecord<IPipelineRecord>>> ExecuteAsync(
        List<TransformedRecord<IPipelineRecord>> records)
    {
        var valid = new List<TransformedRecord<IPipelineRecord>>();
        var rejected = new List<RejectedRecord<IPipelineRecord>>();

        foreach (var record in records)
        {
            // LINQ pipeline to collect all errors for this record:
            // `.Where()` — keep only validators that declare themselves applicable to this record type.
            // `.SelectMany()` — each validator returns a list of errors; SelectMany flattens those
            //   lists into one sequence (e.g., [[err1, err2], [err3]] → [err1, err2, err3]).
            // `.ToList()` — materializes the lazy query so we can check Count and store it.
            var errors = _validators
                .Where(v => v.AppliesTo(record.Record))
                .SelectMany(v => v.Validate(record.Record))
                .ToList();

            // All errors from all matching validators are collected before deciding.
            // This "collect all" approach means a rejected record shows every problem at once,
            // not just the first one — useful for debugging a bad data file.
            if (errors.Count == 0)
            {
                valid.Add(record);
            }
            else
            {
                rejected.Add(new RejectedRecord<IPipelineRecord>(record.Record, errors));
            }
        }

        // Only write if there's something to write. Avoids creating empty rejected files.
        if (rejected.Count > 0)
        {
            await _rejectedWriter.WriteAsync(rejected, _sourceType, _sourceName);
        }

        return valid;
    }
}
