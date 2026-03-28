using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Feedpipe.Core.Models;
using Feedpipe.Core.Services;
using Feedpipe.Services;

namespace Feedpipe.Tests;

public class JsonFeedWriterTests : IDisposable
{
    private readonly string _tempDir;

    public JsonFeedWriterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"hello-dotnet-tests-{Guid.NewGuid()}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task WriteAsync_CreatesOutputDirectory()
    {
        var writer = new JsonFeedWriter(_tempDir, NullLogger<JsonFeedWriter>.Instance);

        await writer.WriteAsync([], "test-feed");

        Assert.True(Directory.Exists(_tempDir));
    }

    [Fact]
    public async Task WriteAsync_WritesJsonFile()
    {
        var writer = new JsonFeedWriter(_tempDir, NullLogger<JsonFeedWriter>.Instance);
        var items = new List<FeedItem>
        {
            new("Test Title", "https://example.com", "A description", new DateTime(2024, 1, 1))
        };

        await writer.WriteAsync(items, "test-feed");

        var files = Directory.GetFiles(_tempDir, "*.json");
        Assert.Single(files);
    }

    [Fact]
    public async Task WriteAsync_FileContainsCorrectData()
    {
        var writer = new JsonFeedWriter(_tempDir, NullLogger<JsonFeedWriter>.Instance);
        var items = new List<FeedItem>
        {
            new("Test Title", "https://example.com", "A description", new DateTime(2024, 1, 1))
        };

        await writer.WriteAsync(items, "test-feed");

        var file = Directory.GetFiles(_tempDir, "*.json").Single();
        var json = await File.ReadAllTextAsync(file);
        var deserialized = JsonSerializer.Deserialize<List<FeedItem>>(json);

        Assert.NotNull(deserialized);
        Assert.Single(deserialized);
        Assert.Equal("Test Title", deserialized[0].Title);
        Assert.Equal("https://example.com", deserialized[0].Link);
    }

    [Fact]
    public async Task WriteAsync_FilenameContainsFeedName()
    {
        var writer = new JsonFeedWriter(_tempDir, NullLogger<JsonFeedWriter>.Instance);

        await writer.WriteAsync([], "my-feed");

        var file = Path.GetFileName(Directory.GetFiles(_tempDir).Single());
        Assert.StartsWith("my-feed_", file);
        Assert.EndsWith(".json", file);
    }
}
