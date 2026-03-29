// -----------------------------------------------------------------------
// FeedSourceAdapter Tests
//
// Unit tests for the feed source adapter. Tests verify that the adapter
// correctly parses both RSS and Atom XML into FeedItem records, and
// auto-detects the format from the XML root element.
// -----------------------------------------------------------------------

using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Conduit.Core.Models;
using Conduit.Core.Services;
using Conduit.Sources.Rss.Services;
using Moq;
using Moq.Protected;

namespace Conduit.Sources.Rss.Tests;

/// <summary>
/// Tests for <see cref="FeedSourceAdapter"/> RSS/Atom parsing and auto-detection.
/// </summary>
public class FeedSourceAdapterTests
{
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

    private const string SampleAtom = """
        <?xml version="1.0" encoding="UTF-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom">
          <title>Test Atom Feed</title>
          <entry>
            <title>Atom Post One</title>
            <link href="https://example.com/atom/1" />
            <summary>First atom summary</summary>
            <updated>2024-01-15T10:00:00Z</updated>
          </entry>
          <entry>
            <title>Atom Post Two</title>
            <link href="https://example.com/atom/2" />
            <summary>&lt;b&gt;Bold summary&lt;/b&gt;</summary>
            <updated>2024-01-16T12:00:00Z</updated>
          </entry>
        </feed>
        """;

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

    // ---- RSS TESTS ----

    [Fact]
    public async Task IngestAsync_ParsesItemsFromRss()
    {
        var httpClient = CreateMockHttpClient(SampleRss);
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");

        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task IngestAsync_ExtractsTitleAndLinkFromRss()
    {
        var httpClient = CreateMockHttpClient(SampleRss);
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");
        var feedItems = items.Cast<FeedItem>().ToList();

        Assert.Equal("First Post", feedItems[0].Title);
        Assert.Equal("https://example.com/1", feedItems[0].Link);
        Assert.Equal("Second Post", feedItems[1].Title);
        Assert.Equal("https://example.com/2", feedItems[1].Link);
    }

    [Fact]
    public async Task IngestAsync_StripsHtmlFromRssDescription()
    {
        var httpClient = CreateMockHttpClient(SampleRss);
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");
        var feedItems = items.Cast<FeedItem>().ToList();

        Assert.Equal("Hello world", feedItems[0].Description);
        Assert.Equal("No HTML here", feedItems[1].Description);
    }

    [Fact]
    public async Task IngestAsync_ParsesPublishedDateFromRss()
    {
        var httpClient = CreateMockHttpClient(SampleRss);
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");
        var feedItems = items.Cast<FeedItem>().ToList();

        Assert.Equal(new DateTime(2024, 1, 1, 12, 0, 0), feedItems[0].PublishedDate.ToUniversalTime());
    }

    [Fact]
    public async Task IngestAsync_HandlesEmptyRssFeed()
    {
        var emptyRss = """
            <?xml version="1.0"?>
            <rss version="2.0">
              <channel><title>Empty</title></channel>
            </rss>
            """;
        var httpClient = CreateMockHttpClient(emptyRss);
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");

        Assert.Empty(items);
    }

    [Fact]
    public async Task IngestAsync_HandlesMissingRssFields()
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
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");

        Assert.Single(items);
        var feedItem = (FeedItem)items[0];
        Assert.Equal("Only Title", feedItem.Title);
        Assert.Equal("", feedItem.Link);
        Assert.Equal("", feedItem.Description);
        Assert.Equal(DateTime.MinValue, feedItem.PublishedDate);
    }

    // ---- ATOM TESTS ----

    [Fact]
    public async Task IngestAsync_ParsesItemsFromAtom()
    {
        var httpClient = CreateMockHttpClient(SampleAtom);
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/atom.xml");

        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task IngestAsync_ExtractsTitleAndLinkFromAtom()
    {
        var httpClient = CreateMockHttpClient(SampleAtom);
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/atom.xml");
        var feedItems = items.Cast<FeedItem>().ToList();

        Assert.Equal("Atom Post One", feedItems[0].Title);
        Assert.Equal("https://example.com/atom/1", feedItems[0].Link);
        Assert.Equal("Atom Post Two", feedItems[1].Title);
        Assert.Equal("https://example.com/atom/2", feedItems[1].Link);
    }

    [Fact]
    public async Task IngestAsync_StripsHtmlFromAtomSummary()
    {
        var httpClient = CreateMockHttpClient(SampleAtom);
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/atom.xml");
        var feedItems = items.Cast<FeedItem>().ToList();

        Assert.Equal("First atom summary", feedItems[0].Description);
        Assert.Equal("Bold summary", feedItems[1].Description);
    }

    [Fact]
    public async Task IngestAsync_ParsesUpdatedDateFromAtom()
    {
        var httpClient = CreateMockHttpClient(SampleAtom);
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/atom.xml");
        var feedItems = items.Cast<FeedItem>().ToList();

        Assert.Equal(new DateTime(2024, 1, 15, 10, 0, 0), feedItems[0].PublishedDate.ToUniversalTime());
    }

    [Fact]
    public async Task IngestAsync_HandlesEmptyAtomFeed()
    {
        var emptyAtom = """
            <?xml version="1.0"?>
            <feed xmlns="http://www.w3.org/2005/Atom">
              <title>Empty</title>
            </feed>
            """;
        var httpClient = CreateMockHttpClient(emptyAtom);
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/atom.xml");

        Assert.Empty(items);
    }

    // ---- AUTO-DETECTION TESTS ----

    [Fact]
    public async Task IngestAsync_AutoDetectsRssFormat()
    {
        var httpClient = CreateMockHttpClient(SampleRss);
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");
        var feedItems = items.Cast<FeedItem>().ToList();

        // RSS items have pubDate-style dates
        Assert.Equal("First Post", feedItems[0].Title);
    }

    [Fact]
    public async Task IngestAsync_AutoDetectsAtomFormat()
    {
        var httpClient = CreateMockHttpClient(SampleAtom);
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");
        var feedItems = items.Cast<FeedItem>().ToList();

        // Atom items have entry/link[@href] style links
        Assert.Equal("https://example.com/atom/1", feedItems[0].Link);
    }

    [Fact]
    public async Task IngestAsync_ReturnsEmptyForUnknownFormat()
    {
        var unknownXml = """
            <?xml version="1.0"?>
            <html><body>Not a feed</body></html>
            """;
        var httpClient = CreateMockHttpClient(unknownXml);
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/page");

        Assert.Empty(items);
    }
}
