using Conduit.Sources.Zotero.Models;
using Conduit.Transforms;

namespace Conduit.Transforms.Tests;

public class ResearchRecordValidatorTests
{
    private readonly ResearchRecordValidator _validator = new();

    private static ResearchRecord Valid() =>
        new("Deep Learning Survey", "LeCun; Bengio", "10.1234/example", "https://example.com/paper",
            "A comprehensive survey of deep learning methods.", "machine learning; neural networks",
            AccessLevel.Open, "");

    [Fact]
    public void AppliesTo_ResearchRecord_ReturnsTrue()
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
    public void EmptyTitle_ReturnsError()
    {
        var record = Valid() with { Title = "" };
        Assert.Contains(_validator.Validate(record), e => e.Contains("Title"));
    }

    [Fact]
    public void NoDoiAndNoUrl_ReturnsError()
    {
        var record = Valid() with { Doi = "", Url = "" };
        Assert.Contains(_validator.Validate(record), e => e.Contains("identifier"));
    }

    [Fact]
    public void InvalidDoiFormat_ReturnsError()
    {
        var record = Valid() with { Doi = "not-a-doi" };
        Assert.Contains(_validator.Validate(record), e => e.Contains("DOI"));
    }

    [Theory]
    [InlineData("10.1234/example")]
    [InlineData("10.1145/3292500.3330919")]
    [InlineData("10.48550/arXiv.2303.08774")]
    public void ValidDoiFormats_NoError(string doi)
    {
        var record = Valid() with { Doi = doi };
        Assert.DoesNotContain(_validator.Validate(record), e => e.Contains("DOI"));
    }

    [Fact]
    public void EmptyAbstractWithOpenAccess_ReturnsWarning()
    {
        var record = Valid() with { Abstract = "", AccessLevel = AccessLevel.Open };
        Assert.Contains(_validator.Validate(record), e => e.Contains("Abstract"));
    }

    [Fact]
    public void EmptyAbstractWithPaywalled_NoWarning()
    {
        var record = Valid() with { Abstract = "", AccessLevel = AccessLevel.Paywalled };
        Assert.DoesNotContain(_validator.Validate(record), e => e.Contains("Abstract"));
    }
}
