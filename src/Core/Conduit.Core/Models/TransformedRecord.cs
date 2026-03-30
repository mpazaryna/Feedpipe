namespace Conduit.Core.Models;

/// <summary>
/// Wraps a raw pipeline record with enrichment metadata produced by
/// transformation stages. The original record is preserved untouched;
/// derived fields live in the <see cref="Enrichment"/> dictionary.
/// </summary>
/// <typeparam name="T">The concrete record type (e.g., FeedItem, EnrollmentRecord).</typeparam>
public class TransformedRecord<T> where T : IPipelineRecord
{
    /// <summary>
    /// The original record from the source adapter, unmodified.
    /// </summary>
    public T Record { get; }

    /// <summary>
    /// Derived fields added by enrichment stages (e.g., keywords, enrollment status).
    /// </summary>
    public Dictionary<string, object> Enrichment { get; }

    /// <summary>
    /// Creates a new transformed record wrapping the given source record.
    /// </summary>
    /// <param name="record">The raw record to wrap.</param>
    public TransformedRecord(T record)
    {
        Record = record;
        Enrichment = new Dictionary<string, object>();
    }
}
