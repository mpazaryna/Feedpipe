// -----------------------------------------------------------------------
// RssFeedFetcher Tests
//
// Unit tests for the RSS feed parsing logic. These tests verify that
// RssFeedFetcher correctly parses RSS XML into FeedItem records.
//
// TESTING STRATEGY:
//
// We mock the HTTP layer so tests never hit the network. This makes them:
//   - Fast (no network latency)
//   - Deterministic (same input = same output, always)
//   - Runnable offline (CI servers, planes, coffee shops)
//
// HOW HTTP MOCKING WORKS IN .NET:
//
// HttpClient delegates all requests to an internal HttpMessageHandler.
// We use Moq to create a fake handler that returns canned XML responses.
// This is the standard pattern for testing HTTP-dependent code in .NET.
//
// The .Protected() call is needed because SendAsync is a protected method
// on HttpMessageHandler -- Moq can't mock it directly, so we use the
// Protected() helper to reach it.
//
// NullLogger<T>.Instance is a built-in no-op logger from Microsoft.
// It satisfies the ILogger<T> dependency without producing any output,
// keeping test output clean.
//
// NAMING CONVENTION:
//
// Test methods follow the pattern: MethodUnderTest_Scenario.
// This is the most common convention in .NET testing. The underscores
// make test names readable in test runner output.
// -----------------------------------------------------------------------

using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Feedpipe.Core.Services;
using Feedpipe.Services;
using Moq;
using Moq.Protected;

namespace Feedpipe.Tests;

/// <summary>
/// Tests for <see cref="RssFeedFetcher"/> RSS parsing and error handling.
/// </summary>
public class RssFeedFetcherTests
{
    /// <summary>
    /// Sample RSS 2.0 XML used across multiple tests. Defined as a constant
    /// so each test works with the same known input. The raw string literal
    /// (triple quotes, C# 11) preserves formatting without escape characters.
    /// </summary>
    private const string SampleRss = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0">
          <channel>
            <title>Test Feed</title>
            <item>
              <title>First Post</title>
              <link>https://example.com/1</link>
              <description>&lt;p&gt;Hello world&lt;/p&gt;</description>
              <pubDate>Mon, 01 Jan 2024 12:00:00 GMT</pubDate>
            </item>
            <item>
              <title>Second Post</title>
              <link>https://example.com/2</link>
              <description>No HTML here</description>
              <pubDate>Tue, 02 Jan 2024 12:00:00 GMT</pubDate>
            </item>
          </channel>
        </rss>
        """;

    /// <summary>
    /// Creates an HttpClient backed by a mocked handler that returns the
    /// given content for any request. This avoids hitting the network.
    /// </summary>
    /// <param name="responseContent">The XML string to return as the response body.</param>
    /// <returns>An HttpClient that will return the canned response.</returns>
    private static HttpClient CreateMockHttpClient(string responseContent)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/xml")
            });

        return new HttpClient(handler.Object);
    }

    /// <summary>
    /// Verifies that the fetcher parses the correct number of items from valid RSS.
    /// </summary>
    [Fact]
    public async Task FetchAsync_ParsesItemsFromRss()
    {
        var httpClient = CreateMockHttpClient(SampleRss);
        var fetcher = new RssFeedFetcher(httpClient, NullLogger<RssFeedFetcher>.Instance);

        var items = await fetcher.FetchAsync("https://example.com/feed");

        Assert.Equal(2, items.Count);
    }

    /// <summary>
    /// Verifies that title and link are extracted correctly from each item.
    /// </summary>
    [Fact]
    public async Task FetchAsync_ExtractsTitleAndLink()
    {
        var httpClient = CreateMockHttpClient(SampleRss);
        var fetcher = new RssFeedFetcher(httpClient, NullLogger<RssFeedFetcher>.Instance);

        var items = await fetcher.FetchAsync("https://example.com/feed");

        Assert.Equal("First Post", items[0].Title);
        Assert.Equal("https://example.com/1", items[0].Link);
        Assert.Equal("Second Post", items[1].Title);
        Assert.Equal("https://example.com/2", items[1].Link);
    }

    /// <summary>
    /// Verifies that HTML tags in descriptions are stripped, leaving plain text.
    /// The first item has <c>&lt;p&gt;</c> tags; the second has no HTML.
    /// </summary>
    [Fact]
    public async Task FetchAsync_StripsHtmlFromDescription()
    {
        var httpClient = CreateMockHttpClient(SampleRss);
        var fetcher = new RssFeedFetcher(httpClient, NullLogger<RssFeedFetcher>.Instance);

        var items = await fetcher.FetchAsync("https://example.com/feed");

        Assert.Equal("Hello world", items[0].Description);
        Assert.Equal("No HTML here", items[1].Description);
    }

    /// <summary>
    /// Verifies that RFC 2822 date strings in pubDate are parsed correctly.
    /// </summary>
    [Fact]
    public async Task FetchAsync_ParsesPublishedDate()
    {
        var httpClient = CreateMockHttpClient(SampleRss);
        var fetcher = new RssFeedFetcher(httpClient, NullLogger<RssFeedFetcher>.Instance);

        var items = await fetcher.FetchAsync("https://example.com/feed");

        Assert.Equal(new DateTime(2024, 1, 1, 12, 0, 0), items[0].PublishedDate.ToUniversalTime());
    }

    /// <summary>
    /// Verifies that a valid RSS feed with no items returns an empty list
    /// rather than throwing an exception.
    /// </summary>
    [Fact]
    public async Task FetchAsync_HandlesEmptyFeed()
    {
        var emptyRss = """
            <?xml version="1.0"?>
            <rss version="2.0">
              <channel><title>Empty</title></channel>
            </rss>
            """;
        var httpClient = CreateMockHttpClient(emptyRss);
        var fetcher = new RssFeedFetcher(httpClient, NullLogger<RssFeedFetcher>.Instance);

        var items = await fetcher.FetchAsync("https://example.com/feed");

        Assert.Empty(items);
    }

    /// <summary>
    /// Verifies graceful handling of items with missing optional elements.
    /// Real-world RSS feeds frequently omit link, description, or pubDate.
    /// The fetcher should use sensible defaults rather than throwing.
    /// </summary>
    [Fact]
    public async Task FetchAsync_HandlesMissingFields()
    {
        var partialRss = """
            <?xml version="1.0"?>
            <rss version="2.0">
              <channel>
                <item>
                  <title>Only Title</title>
                </item>
              </channel>
            </rss>
            """;
        var httpClient = CreateMockHttpClient(partialRss);
        var fetcher = new RssFeedFetcher(httpClient, NullLogger<RssFeedFetcher>.Instance);

        var items = await fetcher.FetchAsync("https://example.com/feed");

        Assert.Single(items);
        Assert.Equal("Only Title", items[0].Title);
        Assert.Equal("", items[0].Link);
        Assert.Equal("", items[0].Description);
        Assert.Equal(DateTime.MinValue, items[0].PublishedDate);
    }
}
