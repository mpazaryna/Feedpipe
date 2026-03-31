using Conduit.Core.Models;
using Conduit.Core.Services;
using Conduit.Sources.Edi834.Models;

namespace Conduit.Transforms;

/// <summary>
/// Validates EDI 834 enrollment records against X12 business rules.
/// </summary>
public class EnrollmentRecordValidator : IRecordValidator
{
    private static readonly HashSet<string> ValidMaintenanceCodes = ["021", "024", "001", "025"];
    private static readonly HashSet<string> ValidRelationshipCodes = ["18", "01", "19", "20", "39", "G8"];

    /// <inheritdoc />
    public bool AppliesTo(IPipelineRecord record) => record is EnrollmentRecord;

    /// <inheritdoc />
    public IReadOnlyList<string> Validate(IPipelineRecord record)
    {
        var enrollment = (EnrollmentRecord)record;
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(enrollment.MemberId))
            errors.Add("MemberId is required");

        if (string.IsNullOrWhiteSpace(enrollment.SubscriberId))
            errors.Add("SubscriberId is required");

        if (string.IsNullOrWhiteSpace(enrollment.MemberName))
            errors.Add("MemberName is required");

        if (string.IsNullOrWhiteSpace(enrollment.PlanId))
            errors.Add("PlanId is required");

        if (!ValidMaintenanceCodes.Contains(enrollment.MaintenanceTypeCode))
            errors.Add($"MaintenanceTypeCode '{enrollment.MaintenanceTypeCode}' is not a valid X12 code (expected: 021, 024, 001, 025)");

        if (!ValidRelationshipCodes.Contains(enrollment.RelationshipCode))
            errors.Add($"RelationshipCode '{enrollment.RelationshipCode}' is not a valid X12 code (expected: 18, 01, 19, 20, 39, G8)");

        if (enrollment.CoverageEndDate.HasValue && enrollment.CoverageEndDate <= enrollment.CoverageStartDate)
            errors.Add($"CoverageEndDate '{enrollment.CoverageEndDate:yyyy-MM-dd}' must be after CoverageStartDate '{enrollment.CoverageStartDate:yyyy-MM-dd}'");

        if (enrollment.CoverageStartDate > DateTime.UtcNow.AddYears(1))
            errors.Add($"CoverageStartDate '{enrollment.CoverageStartDate:yyyy-MM-dd}' is more than 1 year in the future");

        return errors;
    }
}
