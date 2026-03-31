using Conduit.Core.Models;
using Conduit.Core.Services;
using Conduit.Services;
using Conduit.Sources.Edi834.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace Conduit.Transforms.Tests;

public class DeduplicationTransformTests
{
    [Fact]
    public async Task Filters_Duplicate_Records_By_Id()
    {
        var stage = new DeduplicationTransform();
        var records = WrapRecords(
            new FeedItem("Article 1", "https://example.com/1", "Desc", DateTime.UtcNow),
            new FeedItem("Article 1 Copy", "https://example.com/1", "Desc copy", DateTime.UtcNow),
            new FeedItem("Article 2", "https://example.com/2", "Desc", DateTime.UtcNow)
        );

        var result = await stage.ExecuteAsync(records);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Record.Id == "https://example.com/1");
        Assert.Contains(result, r => r.Record.Id == "https://example.com/2");
    }

    [Fact]
    public async Task Keeps_First_Occurrence_Of_Duplicate()
    {
        var stage = new DeduplicationTransform();
        var records = WrapRecords(
            new FeedItem("First Title", "https://example.com/1", "First desc", DateTime.UtcNow),
            new FeedItem("Second Title", "https://example.com/1", "Second desc", DateTime.UtcNow)
        );

        var result = await stage.ExecuteAsync(records);

        Assert.Single(result);
        Assert.Equal("First Title", ((FeedItem)result[0].Record).Title);
    }

    [Fact]
    public async Task Unique_Records_All_Pass_Through()
    {
        var stage = new DeduplicationTransform();
        var records = WrapRecords(
            new FeedItem("A", "https://example.com/1", "D", DateTime.UtcNow),
            new FeedItem("B", "https://example.com/2", "D", DateTime.UtcNow),
            new FeedItem("C", "https://example.com/3", "D", DateTime.UtcNow)
        );

        var result = await stage.ExecuteAsync(records);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Empty_Input_Returns_Empty_Output()
    {
        var stage = new DeduplicationTransform();
        var records = new List<TransformedRecord<IPipelineRecord>>();

        var result = await stage.ExecuteAsync(records);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Preserves_Enrichment_From_Previous_Stages()
    {
        var stage = new DeduplicationTransform();
        var record = new TransformedRecord<IPipelineRecord>(
            new FeedItem("Title", "https://example.com/1", "Desc", DateTime.UtcNow));
        record.Enrichment["existingKey"] = "existingValue";

        var result = await stage.ExecuteAsync([record]);

        Assert.Single(result);
        Assert.Equal("existingValue", result[0].Enrichment["existingKey"]);
    }

    [Fact]
    public async Task Idempotent_On_Repeated_Runs()
    {
        var stage = new DeduplicationTransform();
        var records = WrapRecords(
            new FeedItem("A", "https://example.com/1", "D", DateTime.UtcNow),
            new FeedItem("B", "https://example.com/1", "D", DateTime.UtcNow),
            new FeedItem("C", "https://example.com/2", "D", DateTime.UtcNow)
        );

        var firstRun = await stage.ExecuteAsync(records);
        var secondRun = await stage.ExecuteAsync(WrapRecords(
            new FeedItem("A", "https://example.com/1", "D", DateTime.UtcNow),
            new FeedItem("B", "https://example.com/1", "D", DateTime.UtcNow),
            new FeedItem("C", "https://example.com/2", "D", DateTime.UtcNow)
        ));

        Assert.Equal(firstRun.Count, secondRun.Count);
    }

    [Fact]
    public async Task Edi834_Uses_Composite_Key()
    {
        // Same subscriber but different plan = two distinct records
        var stage = new DeduplicationTransform();
        var records = WrapRecords(
            new EnrollmentRecord("SUB001", "SUB001", true, "Doe, Jane", "18", "021",
                new DateTime(2026, 1, 1), null, "PLAN-A"),
            new EnrollmentRecord("SUB001", "SUB001", true, "Doe, Jane", "18", "021",
                new DateTime(2026, 1, 1), null, "PLAN-B")
        );

        var result = await stage.ExecuteAsync(records);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Edi834_Dedup_Same_Subscriber_Same_Plan_Same_Date()
    {
        // Same subscriber + plan + start date = duplicate
        var stage = new DeduplicationTransform();
        var records = WrapRecords(
            new EnrollmentRecord("SUB001", "SUB001", true, "Doe, Jane", "18", "021",
                new DateTime(2026, 1, 1), null, "PLAN-A"),
            new EnrollmentRecord("SUB001", "SUB001", true, "Doe, Jane Updated", "18", "024",
                new DateTime(2026, 1, 1), new DateTime(2026, 3, 15), "PLAN-A")
        );

        var result = await stage.ExecuteAsync(records);

        Assert.Single(result);
    }

    [Fact]
    public async Task Cross_Run_Dedup_Filters_Previously_Stored_Records()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"conduit-dedup-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var writer = new JsonTransformedOutputWriter(tempDir,
                Mock.Of<ILogger<JsonTransformedOutputWriter>>());

            // First run: write two records
            var firstBatch = WrapRecords(
                new FeedItem("Article 1", "https://example.com/1", "Desc", DateTime.UtcNow),
                new FeedItem("Article 2", "https://example.com/2", "Desc", DateTime.UtcNow));
            await writer.WriteAsync(firstBatch, "rss", "test-feed");

            // Second run: same records plus a new one
            var stage = new DeduplicationTransform(writer, "rss");
            var secondBatch = WrapRecords(
                new FeedItem("Article 1 Again", "https://example.com/1", "Desc", DateTime.UtcNow),
                new FeedItem("Article 2 Again", "https://example.com/2", "Desc", DateTime.UtcNow),
                new FeedItem("Article 3 New", "https://example.com/3", "Desc", DateTime.UtcNow));

            var result = await stage.ExecuteAsync(secondBatch);

            // Only the new record should pass through
            Assert.Single(result);
            Assert.Equal("https://example.com/3", result[0].Record.Id);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task Cross_Run_Dedup_With_No_Previous_Data_Works_Like_Batch_Dedup()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"conduit-dedup-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var writer = new JsonTransformedOutputWriter(tempDir,
                Mock.Of<ILogger<JsonTransformedOutputWriter>>());

            var stage = new DeduplicationTransform(writer, "rss");
            var records = WrapRecords(
                new FeedItem("A", "https://example.com/1", "D", DateTime.UtcNow),
                new FeedItem("B", "https://example.com/1", "D", DateTime.UtcNow),
                new FeedItem("C", "https://example.com/2", "D", DateTime.UtcNow));

            var result = await stage.ExecuteAsync(records);

            Assert.Equal(2, result.Count);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static List<TransformedRecord<IPipelineRecord>> WrapRecords(
        params IPipelineRecord[] records)
    {
        return records
            .Select(r => new TransformedRecord<IPipelineRecord>(r))
            .ToList();
    }
}
