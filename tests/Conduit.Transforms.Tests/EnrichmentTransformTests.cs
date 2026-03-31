using Conduit.Core.Models;
using Conduit.Core.Services;
using Conduit.Sources.Edi834.Models;
using Conduit.Transforms;

namespace Conduit.Transforms.Tests;

public class RssEnrichmentTransformTests
{
    [Fact]
    public async Task Extracts_Keywords_From_Title()
    {
        var stage = new RssEnrichmentTransform();
        var records = WrapRecords(
            new FeedItem("Kubernetes Security Vulnerability Discovered",
                "https://example.com/1", "A new CVE was found", DateTime.UtcNow));

        var result = await stage.ExecuteAsync(records);

        Assert.Single(result);
        Assert.True(result[0].Enrichment.ContainsKey("keywords"));
        var keywords = (List<string>)result[0].Enrichment["keywords"];
        Assert.Contains("kubernetes", keywords);
        Assert.Contains("security", keywords);
    }

    [Fact]
    public async Task Extracts_Keywords_From_Description()
    {
        var stage = new RssEnrichmentTransform();
        var records = WrapRecords(
            new FeedItem("Brief Update", "https://example.com/1",
                "The machine learning framework now supports distributed training across multiple clusters",
                DateTime.UtcNow));

        var result = await stage.ExecuteAsync(records);

        var keywords = (List<string>)result[0].Enrichment["keywords"];
        Assert.Contains("machine", keywords);
        Assert.Contains("learning", keywords);
    }

    [Fact]
    public async Task Skips_Non_FeedItem_Records()
    {
        var stage = new RssEnrichmentTransform();
        var records = WrapRecords(
            new EnrollmentRecord("SUB001", "SUB001", true, "Doe, Jane", "18", "021",
                new DateTime(2026, 1, 1), null, "PLAN-A"));

        var result = await stage.ExecuteAsync(records);

        Assert.Single(result);
        Assert.False(result[0].Enrichment.ContainsKey("keywords"));
    }

    [Fact]
    public async Task Empty_Title_And_Description_Produces_Empty_Keywords()
    {
        var stage = new RssEnrichmentTransform();
        var records = WrapRecords(
            new FeedItem("", "https://example.com/1", "", DateTime.UtcNow));

        var result = await stage.ExecuteAsync(records);

        var keywords = (List<string>)result[0].Enrichment["keywords"];
        Assert.Empty(keywords);
    }

    [Fact]
    public async Task Preserves_Existing_Enrichment()
    {
        var stage = new RssEnrichmentTransform();
        var record = new TransformedRecord<IPipelineRecord>(
            new FeedItem("AI News", "https://example.com/1", "Desc", DateTime.UtcNow));
        record.Enrichment["existingKey"] = "existingValue";

        var result = await stage.ExecuteAsync([record]);

        Assert.Equal("existingValue", result[0].Enrichment["existingKey"]);
        Assert.True(result[0].Enrichment.ContainsKey("keywords"));
    }

    private static List<TransformedRecord<IPipelineRecord>> WrapRecords(
        params IPipelineRecord[] records)
    {
        return records
            .Select(r => new TransformedRecord<IPipelineRecord>(r))
            .ToList();
    }
}

public class Edi834EnrichmentTransformTests
{
    [Fact]
    public async Task Derives_Active_Status_From_Addition_Code()
    {
        var stage = new Edi834EnrichmentTransform();
        var records = WrapRecords(
            new EnrollmentRecord("SUB001", "SUB001", true, "Doe, Jane", "18", "021",
                new DateTime(2026, 1, 1), null, "PLAN-A"));

        var result = await stage.ExecuteAsync(records);

        Assert.Equal("active", result[0].Enrichment["enrollmentStatus"]);
    }

    [Fact]
    public async Task Derives_Terminated_Status_From_Termination_Code()
    {
        var stage = new Edi834EnrichmentTransform();
        var records = WrapRecords(
            new EnrollmentRecord("SUB001", "SUB001", true, "Doe, Jane", "18", "024",
                new DateTime(2026, 1, 1), new DateTime(2026, 3, 15), "PLAN-A"));

        var result = await stage.ExecuteAsync(records);

        Assert.Equal("terminated", result[0].Enrichment["enrollmentStatus"]);
    }

    [Fact]
    public async Task Derives_Terminated_When_CoverageEndDate_In_Past()
    {
        var stage = new Edi834EnrichmentTransform();
        var records = WrapRecords(
            new EnrollmentRecord("SUB001", "SUB001", true, "Doe, Jane", "18", "021",
                new DateTime(2025, 1, 1), new DateTime(2025, 6, 30), "PLAN-A"));

        var result = await stage.ExecuteAsync(records);

        Assert.Equal("terminated", result[0].Enrichment["enrollmentStatus"]);
    }

    [Fact]
    public async Task Skips_Non_EnrollmentRecord()
    {
        var stage = new Edi834EnrichmentTransform();
        var records = WrapRecords(
            new FeedItem("Title", "https://example.com", "Desc", DateTime.UtcNow));

        var result = await stage.ExecuteAsync(records);

        Assert.Single(result);
        Assert.False(result[0].Enrichment.ContainsKey("enrollmentStatus"));
    }

    [Fact]
    public async Task Derives_Relationship_Label()
    {
        var stage = new Edi834EnrichmentTransform();
        var records = WrapRecords(
            new EnrollmentRecord("SUB001", "SUB001", true, "Doe, Jane", "18", "021",
                new DateTime(2026, 1, 1), null, "PLAN-A"),
            new EnrollmentRecord("SUB002", "SUB001", false, "Doe, John", "01", "021",
                new DateTime(2026, 1, 1), null, "PLAN-A"),
            new EnrollmentRecord("SUB003", "SUB001", false, "Doe, Jimmy", "19", "021",
                new DateTime(2026, 1, 1), null, "PLAN-A"));

        var result = await stage.ExecuteAsync(records);

        Assert.Equal("self", result[0].Enrichment["relationship"]);
        Assert.Equal("spouse", result[1].Enrichment["relationship"]);
        Assert.Equal("child", result[2].Enrichment["relationship"]);
    }

    private static List<TransformedRecord<IPipelineRecord>> WrapRecords(
        params IPipelineRecord[] records)
    {
        return records
            .Select(r => new TransformedRecord<IPipelineRecord>(r))
            .ToList();
    }
}

public class ZoteroEnrichmentTransformTests
{
    [Fact]
    public async Task Extracts_Domain_Tags_From_Abstract()
    {
        var stage = new ZoteroEnrichmentTransform();
        var records = WrapRecords(
            new Conduit.Sources.Zotero.Models.ResearchRecord(
                "Deep Learning for Protein Structure",
                "Smith, J; Doe, A",
                "10.1234/test",
                "https://doi.org/10.1234/test",
                "We apply deep learning techniques to predict protein folding structures using neural networks trained on molecular dynamics simulations.",
                "ml;biology",
                Conduit.Sources.Zotero.Models.AccessLevel.Open,
                ""));

        var result = await stage.ExecuteAsync(records);

        Assert.True(result[0].Enrichment.ContainsKey("domainTags"));
        var tags = (List<string>)result[0].Enrichment["domainTags"];
        Assert.NotEmpty(tags);
    }

    [Fact]
    public async Task Skips_Non_ResearchRecord()
    {
        var stage = new ZoteroEnrichmentTransform();
        var records = WrapRecords(
            new FeedItem("Title", "https://example.com", "Desc", DateTime.UtcNow));

        var result = await stage.ExecuteAsync(records);

        Assert.Single(result);
        Assert.False(result[0].Enrichment.ContainsKey("domainTags"));
    }

    [Fact]
    public async Task Empty_Abstract_Produces_Empty_Tags()
    {
        var stage = new ZoteroEnrichmentTransform();
        var records = WrapRecords(
            new Conduit.Sources.Zotero.Models.ResearchRecord(
                "Some Paper", "Author", "10.1234/test", "https://url.com",
                "", "tag1", Conduit.Sources.Zotero.Models.AccessLevel.Unknown, ""));

        var result = await stage.ExecuteAsync(records);

        var tags = (List<string>)result[0].Enrichment["domainTags"];
        Assert.Empty(tags);
    }

    private static List<TransformedRecord<IPipelineRecord>> WrapRecords(
        params IPipelineRecord[] records)
    {
        return records
            .Select(r => new TransformedRecord<IPipelineRecord>(r))
            .ToList();
    }
}
