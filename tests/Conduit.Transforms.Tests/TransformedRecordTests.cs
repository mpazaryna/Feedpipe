using Conduit.Core.Models;

namespace Conduit.Transforms.Tests;

public class TransformedRecordTests
{
    [Fact]
    public void Wraps_Record_Without_Modification()
    {
        var item = new FeedItem("Test Title", "https://example.com/1", "A description", DateTime.UtcNow);

        var transformed = new TransformedRecord<FeedItem>(item);

        Assert.Same(item, transformed.Record);
        Assert.Equal(item.Title, transformed.Record.Title);
        Assert.Equal(item.Link, transformed.Record.Link);
    }

    [Fact]
    public void Enrichment_Is_Empty_By_Default()
    {
        var item = new FeedItem("Title", "https://example.com", "Desc", DateTime.UtcNow);

        var transformed = new TransformedRecord<FeedItem>(item);

        Assert.NotNull(transformed.Enrichment);
        Assert.Empty(transformed.Enrichment);
    }

    [Fact]
    public void Enrichment_Stores_Arbitrary_Values()
    {
        var item = new FeedItem("AI Breakthrough", "https://example.com/ai", "Desc", DateTime.UtcNow);
        var transformed = new TransformedRecord<FeedItem>(item);

        transformed.Enrichment["keywords"] = new List<string> { "ai", "ml" };
        transformed.Enrichment["category"] = "technology";

        Assert.Equal(2, transformed.Enrichment.Count);
        Assert.IsType<List<string>>(transformed.Enrichment["keywords"]);
        Assert.Equal("technology", transformed.Enrichment["category"]);
    }

    [Fact]
    public void Preserves_IPipelineRecord_Properties_Via_Record()
    {
        var item = new FeedItem("Title", "https://example.com/1", "Desc", new DateTime(2026, 3, 29));

        var transformed = new TransformedRecord<FeedItem>(item);

        Assert.Equal("https://example.com/1", transformed.Record.Id);
        Assert.Equal("rss", transformed.Record.SourceType);
        Assert.Equal(new DateTime(2026, 3, 29), transformed.Record.Timestamp);
    }

    [Fact]
    public void Works_With_EnrollmentRecord()
    {
        // Verify the envelope works with immutable positional record types
        var enrollment = new Conduit.Sources.Edi834.Models.EnrollmentRecord(
            "SUB001", "SUB001", true, "Doe, Jane", "18", "021",
            new DateTime(2026, 1, 1), null, "PLAN-A");

        var transformed = new TransformedRecord<Conduit.Sources.Edi834.Models.EnrollmentRecord>(enrollment);

        transformed.Enrichment["enrollmentStatus"] = "active";

        Assert.Same(enrollment, transformed.Record);
        Assert.Equal("active", transformed.Enrichment["enrollmentStatus"]);
        Assert.Equal("SUB001", transformed.Record.SubscriberId);
    }
}
