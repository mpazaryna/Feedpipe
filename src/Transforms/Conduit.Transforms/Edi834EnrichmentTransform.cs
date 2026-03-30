using Conduit.Core.Models;
using Conduit.Core.Services;
using Conduit.Sources.Edi834.Models;

namespace Conduit.Transforms;

/// <summary>
/// Enrichment stage that derives enrollment status and relationship labels
/// from EDI 834 maintenance type codes and coverage dates.
/// </summary>
public class Edi834EnrichmentTransform : ITransform
{
    private static readonly Dictionary<string, string> RelationshipLabels = new()
    {
        ["18"] = "self",
        ["01"] = "spouse",
        ["19"] = "child",
        ["20"] = "employee",
        ["21"] = "unknown",
        ["39"] = "organ-donor",
        ["40"] = "cadaver-donor",
        ["53"] = "life-partner"
    };

    /// <inheritdoc />
    public Task<List<TransformedRecord<IPipelineRecord>>> ExecuteAsync(
        List<TransformedRecord<IPipelineRecord>> records)
    {
        foreach (var record in records)
        {
            if (record.Record is EnrollmentRecord enrollment)
            {
                record.Enrichment["enrollmentStatus"] = DeriveStatus(enrollment);
                record.Enrichment["relationship"] = DeriveRelationship(enrollment);
            }
        }

        return Task.FromResult(records);
    }

    private static string DeriveStatus(EnrollmentRecord enrollment)
    {
        // Termination code
        if (enrollment.MaintenanceTypeCode == "024")
        {
            return "terminated";
        }

        // Coverage end date in the past
        if (enrollment.CoverageEndDate.HasValue && enrollment.CoverageEndDate.Value < DateTime.UtcNow)
        {
            return "terminated";
        }

        return "active";
    }

    private static string DeriveRelationship(EnrollmentRecord enrollment)
    {
        return RelationshipLabels.TryGetValue(enrollment.RelationshipCode, out var label)
            ? label
            : "unknown";
    }
}
