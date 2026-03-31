using Conduit.Core.Models;
using Conduit.Sources.Edi834.Models;
using Conduit.Transforms;

namespace Conduit.Transforms.Tests;

public class FeedItemValidatorTests
{
    private readonly FeedItemValidator _validator = new();

    private static FeedItem Valid() =>
        new("Great Article", "https://example.com/article", "A good read", new DateTime(2026, 1, 1));

    [Fact]
    public void AppliesTo_FeedItem_ReturnsTrue()
    {
        Assert.True(_validator.AppliesTo(Valid()));
    }

    [Fact]
    public void AppliesTo_OtherRecord_ReturnsFalse()
    {
        var enrollment = new EnrollmentRecord("SUB001", "SUB001", true, "Doe, Jane", "18", "021",
            new DateTime(2026, 1, 1), null, "PLAN-A");
        Assert.False(_validator.AppliesTo(enrollment));
    }

    [Fact]
    public void ValidRecord_ReturnsNoErrors()
    {
        Assert.Empty(_validator.Validate(Valid()));
    }

    [Fact]
    public void EmptyTitle_ReturnsError()
    {
        var record = Valid() with { Title = "" };
        Assert.Contains(_validator.Validate(record), e => e.Contains("Title"));
    }

    [Fact]
    public void EmptyLink_ReturnsError()
    {
        var record = Valid() with { Link = "" };
        Assert.Contains(_validator.Validate(record), e => e.Contains("Link"));
    }

    [Fact]
    public void RelativeUrl_ReturnsError()
    {
        var record = Valid() with { Link = "/path/to/article" };
        Assert.Contains(_validator.Validate(record), e => e.Contains("Link"));
    }

    [Fact]
    public void MalformedUrl_ReturnsError()
    {
        var record = Valid() with { Link = "not a url at all" };
        Assert.Contains(_validator.Validate(record), e => e.Contains("Link"));
    }

    [Fact]
    public void DateTimeMinValue_ReturnsError()
    {
        var record = Valid() with { PublishedDate = DateTime.MinValue };
        Assert.Contains(_validator.Validate(record), e => e.Contains("PublishedDate"));
    }
}
