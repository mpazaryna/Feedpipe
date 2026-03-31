// -----------------------------------------------------------------------
// Edi834SourceAdapter Tests
//
// Unit tests for the EDI 834 source adapter. These tests verify that
// Edi834SourceAdapter correctly parses X12 834 files into EnrollmentRecord
// instances, handles edge cases, and recovers gracefully from errors.
// -----------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Conduit.Core.Models;
using Conduit.Sources.Edi834.Models;
using Conduit.Sources.Edi834.Services;

namespace Conduit.Sources.Edi834.Tests;

/// <summary>
/// Tests for <see cref="Edi834SourceAdapter"/> EDI 834 parsing and error handling.
/// </summary>
public class Edi834SourceAdapterTests
{
    private readonly string _fixturesDir;

    public Edi834SourceAdapterTests()
    {
        _fixturesDir = Path.Combine(AppContext.BaseDirectory, "fixtures");
    }

    [Fact]
    public async Task IngestAsync_ParsesSampleFixture_ReturnsFourRecords()
    {
        var adapter = new Edi834SourceAdapter(NullLogger<Edi834SourceAdapter>.Instance);
        var path = Path.Combine(_fixturesDir, "sample-834.edi");

        var items = await adapter.IngestAsync(path);

        Assert.Equal(4, items.Count);
    }

    [Fact]
    public async Task IngestAsync_ParsesSubscriberRecord()
    {
        var adapter = new Edi834SourceAdapter(NullLogger<Edi834SourceAdapter>.Instance);
        var path = Path.Combine(_fixturesDir, "sample-834.edi");

        var items = await adapter.IngestAsync(path);
        var record = Assert.IsType<EnrollmentRecord>(items[0]);

        Assert.Equal("123456789", record.MemberId);
        Assert.Equal("123456789", record.SubscriberId);
        Assert.True(record.IsSubscriber);
        Assert.Equal("DOE, JOHN", record.MemberName);
        Assert.Equal("18", record.RelationshipCode);
        Assert.Equal("021", record.MaintenanceTypeCode);
        Assert.Equal(new DateTime(2024, 1, 1), record.CoverageStartDate);
        Assert.Null(record.CoverageEndDate);
        Assert.Equal("PLAN001", record.PlanId);
    }

    [Fact]
    public async Task IngestAsync_ParsesDependentRecord()
    {
        var adapter = new Edi834SourceAdapter(NullLogger<Edi834SourceAdapter>.Instance);
        var path = Path.Combine(_fixturesDir, "sample-834.edi");

        var items = await adapter.IngestAsync(path);
        var record = Assert.IsType<EnrollmentRecord>(items[1]);

        Assert.Equal("123456790", record.MemberId);
        Assert.Equal("123456789", record.SubscriberId);  // household subscriber is John Doe
        Assert.False(record.IsSubscriber);
        Assert.Equal("DOE, JANE", record.MemberName);
        Assert.Equal("01", record.RelationshipCode);
        Assert.Equal("021", record.MaintenanceTypeCode);
    }

    [Fact]
    public async Task IngestAsync_ParsesTerminationRecord()
    {
        var adapter = new Edi834SourceAdapter(NullLogger<Edi834SourceAdapter>.Instance);
        var path = Path.Combine(_fixturesDir, "sample-834.edi");

        var items = await adapter.IngestAsync(path);
        var record = Assert.IsType<EnrollmentRecord>(items[2]);

        Assert.Equal("987654321", record.MemberId);
        Assert.Equal("987654321", record.SubscriberId);
        Assert.True(record.IsSubscriber);
        Assert.Equal("SMITH, BOB", record.MemberName);
        Assert.Equal("024", record.MaintenanceTypeCode);
        Assert.Equal(new DateTime(2023, 6, 1), record.CoverageStartDate);
        Assert.Equal(new DateTime(2024, 1, 1), record.CoverageEndDate);
        Assert.Equal("PLAN002", record.PlanId);
    }

    [Fact]
    public async Task IngestAsync_ParsesChildDependentRecord()
    {
        var adapter = new Edi834SourceAdapter(NullLogger<Edi834SourceAdapter>.Instance);
        var path = Path.Combine(_fixturesDir, "sample-834.edi");

        var items = await adapter.IngestAsync(path);
        var record = Assert.IsType<EnrollmentRecord>(items[3]);

        Assert.Equal("987654322", record.MemberId);
        Assert.Equal("987654321", record.SubscriberId);  // household subscriber is Bob Smith
        Assert.False(record.IsSubscriber);
        Assert.Equal("SMITH, ALICE", record.MemberName);
        Assert.Equal("19", record.RelationshipCode);
    }

    [Fact]
    public async Task IngestAsync_IPipelineRecordProperties_AreCorrect()
    {
        var adapter = new Edi834SourceAdapter(NullLogger<Edi834SourceAdapter>.Instance);
        var path = Path.Combine(_fixturesDir, "sample-834.edi");

        var items = await adapter.IngestAsync(path);
        var record = items[0];

        Assert.Equal("123456789", record.Id);  // Id => MemberId
        Assert.Equal(new DateTime(2024, 1, 1), record.Timestamp);
        Assert.Equal("edi834", record.SourceType);
    }

    [Fact]
    public async Task IngestAsync_FileNotFound_ReturnsEmptyList()
    {
        var adapter = new Edi834SourceAdapter(NullLogger<Edi834SourceAdapter>.Instance);

        var items = await adapter.IngestAsync("/nonexistent/path/file.edi");

        Assert.Empty(items);
    }

    [Fact]
    public void Parse_EmptyContent_ReturnsEmptyList()
    {
        var records = Edi834SourceAdapter.Parse("");

        Assert.Empty(records);
    }

    [Fact]
    public void Parse_MalformedSegments_ReturnsEmptyList()
    {
        var content = "ISA*garbage~GS*garbage~ST*garbage~SE*1*0001~GE*1*1~IEA*1*1~";

        var records = Edi834SourceAdapter.Parse(content);

        Assert.Empty(records);
    }

    [Fact]
    public void Parse_InsMissingRefAndNm1_SkipsIncompleteRecord()
    {
        // INS segment without REF or NM1 -- member cannot be identified
        var content = "ISA*00*~GS*HP*~ST*834*0001~INS*Y*18*021*28~SE*2*0001~GE*1*1~IEA*1*1~";

        var records = Edi834SourceAdapter.Parse(content);

        Assert.Empty(records);
    }

    [Fact]
    public void Parse_InsWithRefButNoNm1_SkipsRecord()
    {
        var content = "ISA*00*~GS*HP*~ST*834*0001~INS*Y*18*021*28~REF*0F*111222333~SE*2*0001~GE*1*1~IEA*1*1~";

        var records = Edi834SourceAdapter.Parse(content);

        Assert.Empty(records);
    }

    [Fact]
    public void Parse_SingleCompleteRecord_ReturnsOneRecord()
    {
        var content = string.Join("~",
            "ISA*00*",
            "GS*HP*SENDER*RECEIVER",
            "ST*834*0001",
            "INS*Y*18*021*28",
            "REF*0F*111222333",
            "DTP*348*D8*20240301",
            "NM1*IL*1*JONES*PAT****34*111222333",
            "HD*021**HLT*PLANX",
            "SE*6*0001",
            "GE*1*1",
            "IEA*1*1") + "~";

        var records = Edi834SourceAdapter.Parse(content);

        Assert.Single(records);
        var enrollment = Assert.IsType<EnrollmentRecord>(records[0]);
        Assert.Equal("111222333", enrollment.MemberId);
        Assert.Equal("111222333", enrollment.SubscriberId);
        Assert.True(enrollment.IsSubscriber);
        Assert.Equal("JONES, PAT", enrollment.MemberName);
        Assert.Equal("PLANX", enrollment.PlanId);
        Assert.Equal(new DateTime(2024, 3, 1), enrollment.CoverageStartDate);
    }
}
