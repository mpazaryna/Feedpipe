using System.Text.Json;
using Conduit.Core.Models;
using Conduit.Core.Services;
using Conduit.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Conduit.Transforms.Tests;

public class JsonTransformedOutputWriterTests : IDisposable
{
    private readonly string _tempDir;
    private readonly JsonTransformedOutputWriter _writer;

    public JsonTransformedOutputWriterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"conduit-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        var logger = Mock.Of<ILogger<JsonTransformedOutputWriter>>();
        _writer = new JsonTransformedOutputWriter(_tempDir, logger);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task WriteAsync_Creates_File_In_SourceType_Directory()
    {
        var records = new List<TransformedRecord<IPipelineRecord>>
        {
            new(new FeedItem("Title", "https://example.com/1", "Desc", DateTime.UtcNow))
        };

        await _writer.WriteAsync(records, "rss", "test-feed");

        var typeDir = Path.Combine(_tempDir, "rss");
        Assert.True(Directory.Exists(typeDir));
        var files = Directory.GetFiles(typeDir, "*.json");
        Assert.Single(files);
    }

    [Fact]
    public async Task WriteAsync_Serializes_Envelope_With_Record_And_Enrichment()
    {
        var record = new TransformedRecord<IPipelineRecord>(
            new FeedItem("Test", "https://example.com/1", "Desc", DateTime.UtcNow));
        record.Enrichment["keywords"] = new List<string> { "ai", "ml" };

        await _writer.WriteAsync([record], "rss", "test-feed");

        var files = Directory.GetFiles(Path.Combine(_tempDir, "rss"), "*.json");
        var json = await File.ReadAllTextAsync(files[0]);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        var first = root[0];
        Assert.True(first.TryGetProperty("record", out _));
        Assert.True(first.TryGetProperty("enrichment", out _));
    }

    [Fact]
    public async Task ReadPreviousIdsAsync_Returns_Empty_When_No_Files()
    {
        var ids = await _writer.ReadPreviousIdsAsync("rss");

        Assert.Empty(ids);
    }

    [Fact]
    public async Task ReadPreviousIdsAsync_Returns_Ids_From_Written_Records()
    {
        var records = new List<TransformedRecord<IPipelineRecord>>
        {
            new(new FeedItem("A", "https://example.com/1", "D", DateTime.UtcNow)),
            new(new FeedItem("B", "https://example.com/2", "D", DateTime.UtcNow))
        };

        await _writer.WriteAsync(records, "rss", "test-feed");

        var ids = await _writer.ReadPreviousIdsAsync("rss");

        Assert.Equal(2, ids.Count);
        Assert.Contains("https://example.com/1", ids);
        Assert.Contains("https://example.com/2", ids);
    }

    [Fact]
    public async Task ReadPreviousIdsAsync_Reads_Across_Multiple_Files()
    {
        var batch1 = new List<TransformedRecord<IPipelineRecord>>
        {
            new(new FeedItem("A", "https://example.com/1", "D", DateTime.UtcNow))
        };
        var batch2 = new List<TransformedRecord<IPipelineRecord>>
        {
            new(new FeedItem("B", "https://example.com/2", "D", DateTime.UtcNow))
        };

        await _writer.WriteAsync(batch1, "rss", "feed-1");
        await Task.Delay(1100); // ensure different timestamp in filename
        await _writer.WriteAsync(batch2, "rss", "feed-2");

        var ids = await _writer.ReadPreviousIdsAsync("rss");

        Assert.Equal(2, ids.Count);
    }

    [Fact]
    public async Task Transformed_Output_Is_Separate_From_Raw()
    {
        // Raw writer writes to the same temp dir
        var rawLogger = Mock.Of<ILogger<JsonOutputWriter>>();
        var rawWriter = new JsonOutputWriter(_tempDir + "-raw", rawLogger);
        Directory.CreateDirectory(_tempDir + "-raw");

        var rawItems = new List<IPipelineRecord>
        {
            new FeedItem("Title", "https://example.com/1", "Desc", DateTime.UtcNow)
        };
        var transformedItems = new List<TransformedRecord<IPipelineRecord>>
        {
            new(new FeedItem("Title", "https://example.com/1", "Desc", DateTime.UtcNow))
        };

        await rawWriter.WriteAsync(rawItems, "rss", "test");
        await _writer.WriteAsync(transformedItems, "rss", "test");

        // Different directories
        Assert.True(Directory.Exists(Path.Combine(_tempDir + "-raw", "rss")));
        Assert.True(Directory.Exists(Path.Combine(_tempDir, "rss")));

        // Cleanup
        Directory.Delete(_tempDir + "-raw", recursive: true);
    }
}
