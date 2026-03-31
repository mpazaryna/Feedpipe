namespace Conduit.Core.Models;

/// <summary>
/// Implement this on record types where uniqueness requires more than
/// just <see cref="IPipelineRecord.Id"/>. The deduplication stage uses
/// <see cref="DedupKey"/> instead of <c>Id</c> when this interface is present.
/// </summary>
/// <remarks>
/// <para>
/// <b>Opt-in interface pattern.</b> Rather than adding a <c>DedupKey</c> property to
/// <see cref="IPipelineRecord"/> (which would force every record type to implement it),
/// this interface is left optional. Types that need composite deduplication implement it;
/// types that don't are unaffected. The deduplication stage uses C# pattern matching to
/// detect the presence at runtime:
/// <code>
/// var key = record.Record is ICompositeDedupKey composite
///     ? composite.DedupKey
///     : record.Record.Id;
/// </code>
/// </para>
/// <para>
/// <b>Why EDI 834 needs this.</b> An RSS article's URL is a natural unique ID. But an
/// enrollment record's subscriber ID alone is not unique — the same subscriber can appear
/// across multiple plans and coverage periods. The composite key combines subscriber ID,
/// plan ID, and coverage start date into a single string that identifies one unique
/// enrollment event.
/// </para>
/// </remarks>
public interface ICompositeDedupKey
{
    /// <summary>
    /// A composite key string that uniquely identifies this record for
    /// deduplication purposes.
    /// </summary>
    string DedupKey { get; }
}
