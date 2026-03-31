using Conduit.Sources.Edi834.Models;
using Conduit.Transforms;

namespace Conduit.Transforms.Tests;

public class EnrollmentRecordValidatorTests
{
    private readonly EnrollmentRecordValidator _validator = new();

    private static EnrollmentRecord Valid() =>
        new("SUB001", "SUB001", true, "Doe, Jane", "18", "021", new DateTime(2026, 1, 1), null, "PLAN-A");

    [Fact]
    public void AppliesTo_EnrollmentRecord_ReturnsTrue()
    {
        Assert.True(_validator.AppliesTo(Valid()));
    }

    [Fact]
    public void AppliesTo_OtherRecord_ReturnsFalse()
    {
        var feedItem = new Conduit.Core.Models.FeedItem("Title", "https://example.com", "desc", DateTime.UtcNow);
        Assert.False(_validator.AppliesTo(feedItem));
    }

    [Fact]
    public void ValidRecord_ReturnsNoErrors()
    {
        Assert.Empty(_validator.Validate(Valid()));
    }

    [Fact]
    public void EmptySubscriberId_ReturnsError()
    {
        var record = Valid() with { SubscriberId = "" };
        Assert.Contains(_validator.Validate(record), e => e.Contains("SubscriberId"));
    }

    [Fact]
    public void EmptyMemberName_ReturnsError()
    {
        var record = Valid() with { MemberName = "" };
        Assert.Contains(_validator.Validate(record), e => e.Contains("MemberName"));
    }

    [Fact]
    public void EmptyPlanId_ReturnsError()
    {
        var record = Valid() with { PlanId = "" };
        Assert.Contains(_validator.Validate(record), e => e.Contains("PlanId"));
    }

    [Fact]
    public void InvalidMaintenanceTypeCode_ReturnsError()
    {
        var record = Valid() with { MaintenanceTypeCode = "999" };
        Assert.Contains(_validator.Validate(record), e => e.Contains("MaintenanceTypeCode"));
    }

    [Theory]
    [InlineData("021")]
    [InlineData("024")]
    [InlineData("001")]
    [InlineData("025")]
    public void ValidMaintenanceTypeCode_NoError(string code)
    {
        var record = Valid() with { MaintenanceTypeCode = code };
        Assert.DoesNotContain(_validator.Validate(record), e => e.Contains("MaintenanceTypeCode"));
    }

    [Fact]
    public void InvalidRelationshipCode_ReturnsError()
    {
        var record = Valid() with { RelationshipCode = "99" };
        Assert.Contains(_validator.Validate(record), e => e.Contains("RelationshipCode"));
    }

    [Fact]
    public void EndDateBeforeStartDate_ReturnsError()
    {
        var record = Valid() with
        {
            CoverageStartDate = new DateTime(2026, 6, 1),
            CoverageEndDate = new DateTime(2026, 1, 1)
        };
        Assert.Contains(_validator.Validate(record), e => e.Contains("CoverageEndDate"));
    }

    [Fact]
    public void StartDateMoreThanOneYearInFuture_ReturnsError()
    {
        var record = Valid() with { CoverageStartDate = DateTime.UtcNow.AddYears(2) };
        Assert.Contains(_validator.Validate(record), e => e.Contains("CoverageStartDate"));
    }
}
