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
/// <param name="SubscriberId">The subscriber identifier from the REF*0F segment (typically SSN).</param>
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
    string SubscriberId,
    string MemberName,
    string RelationshipCode,
    string MaintenanceTypeCode,
    DateTime CoverageStartDate,
    DateTime? CoverageEndDate,
    string PlanId
) : IPipelineRecord, ICompositeDedupKey
{
    /// <inheritdoc />
    public string DedupKey => $"{SubscriberId}|{CoverageStartDate:yyyy-MM-dd}|{PlanId}";

    /// <inheritdoc />
    public string Id => SubscriberId;

    /// <inheritdoc />
    public DateTime Timestamp => CoverageStartDate;

    /// <inheritdoc />
    public string SourceType => "edi834";
}
