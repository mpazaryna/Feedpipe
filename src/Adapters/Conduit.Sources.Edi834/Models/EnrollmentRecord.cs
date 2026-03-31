using Conduit.Core.Models;

namespace Conduit.Sources.Edi834.Models;

/// <summary>
/// Represents a single member enrollment record parsed from an EDI 834 file.
/// </summary>
/// <remarks>
/// <para>
/// Maps to the member-level loop (2000/2100/2300) in an X12 834 transaction.
/// Each record captures one member's enrollment action -- new enrollment,
/// termination, or change -- along with identifying and coverage details.
/// </para>
/// </remarks>
/// <param name="MemberId">The individual member's identifier from the REF*0F segment.</param>
/// <param name="SubscriberId">
/// The household subscriber's identifier (the INS01=Y member's REF*0F).
/// For subscribers, this equals <see cref="MemberId"/>. For dependents, this is the
/// primary subscriber's ID, enabling household grouping.
/// </param>
/// <param name="IsSubscriber">
/// True when INS01 is "Y" (this member is the subscriber); false for dependents (INS01 = "N").
/// </param>
/// <param name="MemberName">Full name from the NM1 segment (Last, First format).</param>
/// <param name="RelationshipCode">
/// INS02 code indicating the member's relationship to the subscriber.
/// Common values: 18 = self, 01 = spouse, 19 = child.
/// </param>
/// <param name="MaintenanceTypeCode">
/// INS03 code indicating the action. Common values: 021 = addition, 024 = termination.
/// </param>
/// <param name="CoverageStartDate">Start of coverage from DTP*348 segment.</param>
/// <param name="CoverageEndDate">End of coverage from DTP*349 segment, if present.</param>
/// <param name="PlanId">Health plan identifier from the HD segment.</param>
public record EnrollmentRecord(
    string MemberId,
    string SubscriberId,
    bool IsSubscriber,
    string MemberName,
    string RelationshipCode,
    string MaintenanceTypeCode,
    DateTime CoverageStartDate,
    DateTime? CoverageEndDate,
    string PlanId
) : IPipelineRecord, ICompositeDedupKey
{
    /// <inheritdoc />
    public string DedupKey => $"{SubscriberId}|{MemberId}|{CoverageStartDate:yyyy-MM-dd}|{PlanId}";

    /// <inheritdoc />
    public string Id => MemberId;

    /// <inheritdoc />
    public DateTime Timestamp => CoverageStartDate;

    /// <inheritdoc />
    public string SourceType => "edi834";
}
