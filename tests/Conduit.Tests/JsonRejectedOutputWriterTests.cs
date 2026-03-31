// -----------------------------------------------------------------------
// JsonRejectedOutputWriter Tests
//
// Unit tests for the rejected-tier JSON writer. Verifies that rejected
// records are serialized with original record, errors, and rejectedAt,
// organized by source type, with the correct filename pattern.
//
// Each test creates a unique temp directory and cleans it up in Dispose().
// -----------------------------------------------------------------------

using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Conduit.Core.Models;
using Conduit.Core.Services;
using Conduit.Services;

namespace Conduit.Tests;

/// <summary>
/// Tests for <see cref="JsonRejectedOutputWriter"/> file output behavior.
/// </summary>
public class JsonRejectedOutputWriterTests : IDisposable
{
    private readonly string _tempDir;
    private readonly JsonRejectedOutputWriter _writer;

    public JsonRejectedOutputWriterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"conduit-rejected-tests-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        _writer = new JsonRejectedOutputWriter(_tempDir, NullLogger<JsonRejectedOutputWriter>.Instance);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task WriteAsync_CreatesSourceTypeSubdirectory()
    {
        await _writer.WriteAsync([], "edi834", "benefits-enrollment");

        Assert.True(Directory.Exists(Path.Combine(_tempDir, "edi834")));
    }

    [Fact]
    public async Task WriteAsync_WritesJsonFileInTypeDirectory()
    {
        var record = new RejectedRecord<IPipelineRecord>(
            new FeedItem("Bad Item", "https://example.com/bad", "desc", DateTime.UtcNow),
            ["Title is empty"]);

        await _writer.WriteAsync([record], "rss", "test-source");

        var files = Directory.GetFiles(Path.Combine(_tempDir, "rss"), "*.json");
        Assert.Single(files);
    }

    [Fact]
    public async Task WriteAsync_SerializesEnvelopeWithRecordErrorsAndRejectedAt()
    {
        var feedItem = new FeedItem("Bad Item", "https://example.com/bad", "desc", DateTime.UtcNow);
        var record = new RejectedRecord<IPipelineRecord>(feedItem, ["Field X is required", "Field Y is invalid"]);

        await _writer.WriteAsync([record], "rss", "test-source");

        var file = Directory.GetFiles(Path.Combine(_tempDir, "rss"), "*.json").Single();
        var json = await File.ReadAllTextAsync(file);
        using var doc = JsonDocument.Parse(json);
        var first = doc.RootElement[0];

        Assert.True(first.TryGetProperty("record", out _));
        Assert.True(first.TryGetProperty("errors", out var errors));
        Assert.True(first.TryGetProperty("rejectedAt", out _));
        Assert.Equal(2, errors.GetArrayLength());
    }

    [Fact]
    public async Task WriteAsync_FilenameStartsWithSourceName()
    {
        await _writer.WriteAsync([], "rss", "my-source");

        var file = Path.GetFileName(Directory.GetFiles(Path.Combine(_tempDir, "rss")).Single());
        Assert.StartsWith("my-source_", file);
        Assert.EndsWith(".json", file);
    }

    [Fact]
    public async Task WriteAsync_DoesNotWriteToRawOrCuratedDirectories()
    {
        var record = new RejectedRecord<IPipelineRecord>(
            new FeedItem("Bad", "https://example.com/1", "desc", DateTime.UtcNow),
            ["Invalid"]);

        await _writer.WriteAsync([record], "rss", "test-source");

        Assert.False(Directory.Exists(Path.Combine(_tempDir, "raw")));
        Assert.False(Directory.Exists(Path.Combine(_tempDir, "curated")));
    }
}
