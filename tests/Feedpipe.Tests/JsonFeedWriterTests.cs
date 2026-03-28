// -----------------------------------------------------------------------
// JsonFeedWriter Tests
//
// Unit tests for the JSON file writing logic. These tests verify that
// JsonFeedWriter correctly serializes FeedItems and manages output files.
//
// TEST ISOLATION:
//
// Each test creates a unique temporary directory (via Guid) and cleans
// it up in Dispose(). This ensures tests don't interfere with each other
// and don't leave artifacts on disk.
//
// In xUnit, the test class constructor runs before each test and Dispose()
// runs after each test. This is xUnit's equivalent of pytest fixtures
// with setup/teardown. The IDisposable interface tells xUnit to call
// Dispose() automatically -- you don't need to wire it up.
//
// GC.SuppressFinalize(this) in Dispose() is a .NET best practice when
// implementing IDisposable. It tells the garbage collector "I've already
// cleaned up, no need to run a finalizer." The code analyzer enforces this.
// -----------------------------------------------------------------------

using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Feedpipe.Core.Models;
using Feedpipe.Core.Services;
using Feedpipe.Services;

namespace Feedpipe.Tests;

/// <summary>
/// Tests for <see cref="JsonFeedWriter"/> file output behavior.
/// </summary>
/// <remarks>
/// Implements <see cref="IDisposable"/> to clean up temporary directories
/// after each test. This is the xUnit pattern for test cleanup -- equivalent
/// to pytest's <c>tmp_path</c> fixture or a teardown method.
/// </remarks>
public class JsonFeedWriterTests : IDisposable
{
    /// <summary>
    /// A unique temporary directory for each test instance. Using a Guid
    /// prevents collisions when tests run in parallel.
    /// </summary>
    private readonly string _tempDir;

    /// <summary>Initializes a fresh temp directory for each test.</summary>
    public JsonFeedWriterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"feedpipe-tests-{Guid.NewGuid()}");
    }

    /// <summary>
    /// Cleans up the temporary directory after each test completes.
    /// The <c>recursive: true</c> parameter deletes the directory and
    /// all its contents.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Verifies that WriteAsync creates the output directory if it doesn't exist.
    /// </summary>
    [Fact]
    public async Task WriteAsync_CreatesOutputDirectory()
    {
        var writer = new JsonFeedWriter(_tempDir, NullLogger<JsonFeedWriter>.Instance);

        await writer.WriteAsync([], "test-feed");

        Assert.True(Directory.Exists(_tempDir));
    }

    /// <summary>
    /// Verifies that WriteAsync creates exactly one JSON file per call.
    /// </summary>
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

    /// <summary>
    /// Verifies that the written JSON can be deserialized back into the
    /// original data. This is a round-trip test -- serialize then deserialize
    /// and compare.
    /// </summary>
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

    /// <summary>
    /// Verifies that the output filename includes the feed name and has
    /// a .json extension. The exact timestamp portion varies, so we only
    /// check the prefix and suffix.
    /// </summary>
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
