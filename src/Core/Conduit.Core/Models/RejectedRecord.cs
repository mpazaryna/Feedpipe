namespace Conduit.Core.Models;

/// <summary>
/// Wraps a pipeline record that failed validation, preserving the original
/// record alongside the reasons it was rejected.
/// </summary>
/// <typeparam name="T">The concrete record type (e.g., FeedItem, EnrollmentRecord).</typeparam>
/// <remarks>
/// <para>
/// <b>Mirrors <see cref="TransformedRecord{T}"/>.</b> Valid records become
/// <c>TransformedRecord&lt;T&gt;</c> envelopes carrying enrichment data; invalid records
/// become <c>RejectedRecord&lt;T&gt;</c> envelopes carrying error messages. The same
/// generic constraint (<c>where T : IPipelineRecord</c>) applies — see
/// <see cref="TransformedRecord{T}"/> remarks for the full explanation.
/// </para>
/// <para>
/// <b><c>IReadOnlyList&lt;string&gt;</c> vs <c>List&lt;string&gt;</c>.</b> The errors are
/// exposed as <c>IReadOnlyList</c> rather than <c>List</c> to express intent: callers
/// can read and iterate the errors but cannot add to or remove them after the record is
/// created. This is a common C# pattern for "I own this collection; you can observe it."
/// </para>
/// <para>
/// <b>Audit timestamp.</b> <c>RejectedAt</c> is set to <c>DateTime.UtcNow</c> in the
/// constructor rather than being passed in. UTC (not local time) is the convention for
/// stored timestamps — it avoids ambiguity when the system moves between time zones or
/// when logs from multiple machines are compared.
/// </para>
/// </remarks>
public class RejectedRecord<T> where T : IPipelineRecord
{
    /// <summary>
    /// The original record from the source adapter, unmodified.
    /// </summary>
    public T Record { get; }

    /// <summary>
    /// Human-readable descriptions of why this record failed validation.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// When this record was rejected by the validation stage.
    /// </summary>
    public DateTime RejectedAt { get; }

    /// <summary>
    /// Creates a new rejected record wrapping the given source record.
    /// </summary>
    /// <param name="record">The raw record that failed validation.</param>
    /// <param name="errors">Human-readable validation failure messages.</param>
    public RejectedRecord(T record, IReadOnlyList<string> errors)
    {
        Record = record;
        Errors = errors;
        RejectedAt = DateTime.UtcNow;
    }
}
