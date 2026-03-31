using Conduit.Core.Models;

namespace Conduit.Core.Services;

/// <summary>
/// Defines a validation rule set for a specific record type.
/// </summary>
/// <remarks>
/// <para>
/// <b>Guard + validate pattern.</b> Two methods instead of one: <see cref="AppliesTo"/>
/// is the guard — it answers "is this validator relevant for this record?" without
/// doing any real work. <see cref="Validate"/> is called only when <c>AppliesTo</c>
/// returns <c>true</c>. This lets the pipeline hold a flat list of all validators
/// and apply each one to each record without caring about types upfront.
/// </para>
/// <para>
/// <b>Error accumulation, not fail-fast.</b> <see cref="Validate"/> returns all
/// errors found, not just the first. When a bad EDI file arrives, you want to see
/// every problem at once — not fix one error, resubmit, and discover the next.
/// </para>
/// <para>
/// <b><c>IReadOnlyList&lt;string&gt;</c> not <c>IEnumerable&lt;string&gt;</c>.</b>
/// Returning <c>IReadOnlyList</c> signals two things: (1) the result is fully
/// materialized — no deferred execution, no side effects on iteration; (2) callers
/// can check <c>Count == 0</c> efficiently without enumerating, which is a common
/// idiom for "is this record valid?"
/// </para>
/// <para>
/// <b>Dependency direction.</b> This interface lives in <c>Conduit.Core</c> so
/// <c>ValidationTransform</c> (also in Core) can depend on it. The concrete
/// validators (<c>FeedItemValidator</c>, etc.) live in <c>Conduit.Transforms</c>
/// where the adapter model types are available. Core never references Transforms.
/// </para>
/// </remarks>
public interface IRecordValidator
{
    /// <summary>
    /// Returns true if this validator applies to the given record type.
    /// </summary>
    bool AppliesTo(IPipelineRecord record);

    /// <summary>
    /// Validates the record and returns a list of human-readable error messages.
    /// An empty list means the record is valid.
    /// </summary>
    IReadOnlyList<string> Validate(IPipelineRecord record);
}
