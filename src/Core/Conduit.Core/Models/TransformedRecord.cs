namespace Conduit.Core.Models;

/// <summary>
/// Wraps a raw pipeline record with enrichment metadata produced by
/// transformation stages. The original record is preserved untouched;
/// derived fields live in the <see cref="Enrichment"/> dictionary.
/// </summary>
/// <typeparam name="T">The concrete record type (e.g., FeedItem, EnrollmentRecord).</typeparam>
/// <remarks>
/// <para>
/// <b>C# generics and type constraints.</b> The <c>&lt;T&gt;</c> makes this class work
/// with any record type while keeping the compiler's type information intact. The
/// <c>where T : IPipelineRecord</c> constraint says "T must implement IPipelineRecord" —
/// without it, the compiler would only know <c>T</c> is <c>object</c> and you couldn't
/// access <c>Id</c>, <c>Timestamp</c>, or <c>SourceType</c> on <c>Record</c>.
/// </para>
/// <para>
/// <b>Envelope / decorator pattern.</b> Rather than mutating the original record (adding
/// fields to <c>FeedItem</c> as enrichment runs), we wrap it in an envelope that carries
/// derived data separately. This keeps source models immutable and makes it easy to
/// compare the raw record with its enriched output.
/// </para>
/// <para>
/// <b>Open enrichment dictionary.</b> <c>Enrichment</c> is a <c>Dictionary&lt;string, object&gt;</c>
/// rather than a typed class because each source type adds different derived fields
/// (RSS adds "keywords"; EDI 834 adds "enrollmentStatus"). A shared typed class would
/// require changing Core every time a new enrichment field is introduced.
/// </para>
/// </remarks>
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
