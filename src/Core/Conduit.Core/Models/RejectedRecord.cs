namespace Conduit.Core.Models;

/// <summary>
/// Wraps a pipeline record that failed validation, preserving the original
/// record alongside the reasons it was rejected.
/// </summary>
/// <typeparam name="T">The concrete record type (e.g., FeedItem, EnrollmentRecord).</typeparam>
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
