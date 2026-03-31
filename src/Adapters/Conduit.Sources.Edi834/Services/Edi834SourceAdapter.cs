using System.Globalization;
using Microsoft.Extensions.Logging;
using Conduit.Core.Models;
using Conduit.Core.Services;
using Conduit.Sources.Edi834.Models;

namespace Conduit.Sources.Edi834.Services;

/// <summary>
/// Ingests and parses EDI 834 (Benefit Enrollment) files from disk.
/// </summary>
/// <remarks>
/// <para>
/// This is a minimal parser covering common enrollment scenarios: subscriber
/// and dependent additions, and terminations. It reads the flat X12 segment
/// stream, groups segments by member (INS loop), and maps INS/REF/DTP/NM1/HD
/// segments into <see cref="EnrollmentRecord"/> instances.
/// </para>
/// <para>
/// A production parser would use a library like OopFactory.X12 or EdiFabric
/// for full spec compliance. This implementation handles the 80% case.
/// </para>
/// </remarks>
public class Edi834SourceAdapter : ISourceAdapter
{
    private readonly ILogger<Edi834SourceAdapter> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="Edi834SourceAdapter"/>.
    /// </summary>
    /// <param name="logger">Typed logger for structured logging.</param>
    public Edi834SourceAdapter(ILogger<Edi834SourceAdapter> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<IPipelineRecord>> IngestAsync(string location)
    {
        _logger.LogInformation("Ingesting EDI 834 source: {Location}", location);

        try
        {
            if (!File.Exists(location))
            {
                _logger.LogError("834 file not found: {Location}", location);
                return [];
            }

            var content = await File.ReadAllTextAsync(location);
            var records = Parse(content);

            _logger.LogInformation("Parsed {Count} enrollment records from {Location}", records.Count, location);
            return records;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogError(ex, "Failed to parse 834 file: {Location}", location);
            return [];
        }
    }

    /// <summary>
    /// Parses raw X12 834 content into enrollment records.
    /// </summary>
    /// <param name="content">The raw EDI 834 file content.</param>
    /// <returns>A list of parsed enrollment records.</returns>
    public static List<IPipelineRecord> Parse(string content)
    {
        var records = new List<IPipelineRecord>();

        // Split on segment terminator (~), trim whitespace and newlines
        var segments = content
            .Split('~', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().TrimStart('\n', '\r'))
            .Where(s => s.Length > 0)
            .ToList();

        // Walk segments, grouping by INS (member loop boundary)
        string? memberId = null;
        string? currentSubscriberId = null;  // tracks the active household subscriber across loops
        bool isSubscriber = false;
        string? memberName = null;
        string? relationshipCode = null;
        string? maintenanceTypeCode = null;
        DateTime coverageStartDate = DateTime.MinValue;
        DateTime? coverageEndDate = null;
        string? planId = null;
        var inMemberLoop = false;

        foreach (var segment in segments)
        {
            var elements = segment.Split('*');
            var segmentId = elements[0];

            if (segmentId == "INS")
            {
                // Flush the previous member if we have one
                if (inMemberLoop && memberId is not null && memberName is not null)
                {
                    // Subscriber establishes the household context for subsequent dependents
                    if (isSubscriber)
                        currentSubscriberId = memberId;

                    records.Add(new EnrollmentRecord(
                        MemberId: memberId,
                        SubscriberId: currentSubscriberId ?? memberId,
                        IsSubscriber: isSubscriber,
                        MemberName: memberName,
                        RelationshipCode: relationshipCode ?? "",
                        MaintenanceTypeCode: maintenanceTypeCode ?? "",
                        CoverageStartDate: coverageStartDate,
                        CoverageEndDate: coverageEndDate,
                        PlanId: planId ?? ""));
                }

                // Start a new member
                inMemberLoop = true;
                memberId = null;
                memberName = null;
                isSubscriber = elements.Length > 1 && elements[1] == "Y";
                relationshipCode = elements.Length > 2 ? elements[2] : null;
                maintenanceTypeCode = elements.Length > 3 ? elements[3] : null;
                coverageStartDate = DateTime.MinValue;
                coverageEndDate = null;
                planId = null;
            }
            else if (segmentId == "REF" && inMemberLoop)
            {
                // REF*0F*{memberId} -- individual member identifier
                if (elements.Length > 2 && elements[1] == "0F")
                {
                    memberId = elements[2];
                }
            }
            else if (segmentId == "DTP" && inMemberLoop)
            {
                // DTP*348*D8*{date} -- coverage start
                // DTP*349*D8*{date} -- coverage end
                if (elements.Length > 3 && elements[2] == "D8")
                {
                    if (elements[1] == "348" &&
                        DateTime.TryParseExact(elements[3], "yyyyMMdd", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out var startDate))
                    {
                        coverageStartDate = startDate;
                    }
                    else if (elements[1] == "349" &&
                        DateTime.TryParseExact(elements[3], "yyyyMMdd", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out var endDate))
                    {
                        coverageEndDate = endDate;
                    }
                }
            }
            else if (segmentId == "NM1" && inMemberLoop)
            {
                // NM1*IL*1*{last}*{first}*...
                if (elements.Length > 4 && elements[1] == "IL")
                {
                    var last = elements[3];
                    var first = elements[4];
                    memberName = $"{last}, {first}";
                }
            }
            else if (segmentId == "HD" && inMemberLoop)
            {
                // HD*021**HLT*{planId}
                if (elements.Length > 4)
                {
                    planId = elements[4];
                }
            }
        }

        // Flush the last member
        if (inMemberLoop && memberId is not null && memberName is not null)
        {
            if (isSubscriber)
                currentSubscriberId = memberId;

            records.Add(new EnrollmentRecord(
                MemberId: memberId,
                SubscriberId: currentSubscriberId ?? memberId,
                IsSubscriber: isSubscriber,
                MemberName: memberName,
                RelationshipCode: relationshipCode ?? "",
                MaintenanceTypeCode: maintenanceTypeCode ?? "",
                CoverageStartDate: coverageStartDate,
                CoverageEndDate: coverageEndDate,
                PlanId: planId ?? ""));
        }

        return records;
    }
}
