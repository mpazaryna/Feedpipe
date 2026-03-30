namespace Conduit.Core.Models;

/// <summary>
/// Implement this on record types where uniqueness requires more than
/// just <see cref="IPipelineRecord.Id"/>. The deduplication stage uses
/// <see cref="DedupKey"/> instead of <c>Id</c> when this interface is present.
/// </summary>
public interface ICompositeDedupKey
{
    /// <summary>
    /// A composite key string that uniquely identifies this record for
    /// deduplication purposes.
    /// </summary>
    string DedupKey { get; }
}
