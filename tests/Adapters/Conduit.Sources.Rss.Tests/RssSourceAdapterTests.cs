// -----------------------------------------------------------------------
// FeedSourceAdapter Tests
//
// Unit tests for the feed source adapter. Tests verify that the adapter
// correctly parses both RSS and Atom XML into FeedItem records, and
// auto-detects the format from the XML root element.
//
// All sample XML is loaded from fixture files in the fixtures/ directory,
// matching the pattern used by the 834 and Zotero test projects.
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
    private static readonly string FixturesDir = Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "fixtures");

    private static string LoadFixture(string filename) =>
        File.ReadAllText(Path.Combine(FixturesDir, filename));

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
        var httpClient = CreateMockHttpClient(LoadFixture("sample-rss.xml"));
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");

        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task IngestAsync_ExtractsTitleAndLinkFromRss()
    {
        var httpClient = CreateMockHttpClient(LoadFixture("sample-rss.xml"));
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
        var httpClient = CreateMockHttpClient(LoadFixture("sample-rss.xml"));
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");
        var feedItems = items.Cast<FeedItem>().ToList();

        Assert.Equal("Hello world", feedItems[0].Description);
        Assert.Equal("No HTML here", feedItems[1].Description);
    }

    [Fact]
    public async Task IngestAsync_ParsesPublishedDateFromRss()
    {
        var httpClient = CreateMockHttpClient(LoadFixture("sample-rss.xml"));
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");
        var feedItems = items.Cast<FeedItem>().ToList();

        Assert.Equal(new DateTime(2024, 1, 1, 12, 0, 0), feedItems[0].PublishedDate.ToUniversalTime());
    }

    [Fact]
    public async Task IngestAsync_HandlesEmptyRssFeed()
    {
        var httpClient = CreateMockHttpClient(LoadFixture("empty-rss.xml"));
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");

        Assert.Empty(items);
    }

    [Fact]
    public async Task IngestAsync_HandlesMissingRssFields()
    {
        var httpClient = CreateMockHttpClient(LoadFixture("partial-rss.xml"));
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
        var httpClient = CreateMockHttpClient(LoadFixture("sample-atom.xml"));
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/atom.xml");

        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task IngestAsync_ExtractsTitleAndLinkFromAtom()
    {
        var httpClient = CreateMockHttpClient(LoadFixture("sample-atom.xml"));
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
        var httpClient = CreateMockHttpClient(LoadFixture("sample-atom.xml"));
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/atom.xml");
        var feedItems = items.Cast<FeedItem>().ToList();

        Assert.Equal("First atom summary", feedItems[0].Description);
        Assert.Equal("Bold summary", feedItems[1].Description);
    }

    [Fact]
    public async Task IngestAsync_ParsesUpdatedDateFromAtom()
    {
        var httpClient = CreateMockHttpClient(LoadFixture("sample-atom.xml"));
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/atom.xml");
        var feedItems = items.Cast<FeedItem>().ToList();

        Assert.Equal(new DateTime(2024, 1, 15, 10, 0, 0), feedItems[0].PublishedDate.ToUniversalTime());
    }

    [Fact]
    public async Task IngestAsync_HandlesEmptyAtomFeed()
    {
        var httpClient = CreateMockHttpClient(LoadFixture("empty-atom.xml"));
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/atom.xml");

        Assert.Empty(items);
    }

    // ---- AUTO-DETECTION TESTS ----

    [Fact]
    public async Task IngestAsync_AutoDetectsRssFormat()
    {
        var httpClient = CreateMockHttpClient(LoadFixture("sample-rss.xml"));
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");
        var feedItems = items.Cast<FeedItem>().ToList();

        Assert.Equal("First Post", feedItems[0].Title);
    }

    [Fact]
    public async Task IngestAsync_AutoDetectsAtomFormat()
    {
        var httpClient = CreateMockHttpClient(LoadFixture("sample-atom.xml"));
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");
        var feedItems = items.Cast<FeedItem>().ToList();

        Assert.Equal("https://example.com/atom/1", feedItems[0].Link);
    }

    [Fact]
    public async Task IngestAsync_ReturnsEmptyForUnknownFormat()
    {
        var httpClient = CreateMockHttpClient(LoadFixture("unknown-format.xml"));
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/page");

        Assert.Empty(items);
    }

    // ---- ERROR HANDLING TESTS ----

    [Fact]
    public async Task IngestAsync_ReturnsEmptyOnHttpError()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var httpClient = new HttpClient(handler.Object);
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://unreachable.example.com/feed");

        Assert.Empty(items);
    }

    [Fact]
    public async Task IngestAsync_ReturnsEmptyOnMalformedXml()
    {
        var httpClient = CreateMockHttpClient("this is not xml at all <><><");
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/broken");

        Assert.Empty(items);
    }

    // ---- ATOM EDGE CASES ----

    [Fact]
    public async Task IngestAsync_HandlesAtomContentFallback()
    {
        var httpClient = CreateMockHttpClient(LoadFixture("atom-with-content.xml"));
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");
        var feedItems = items.Cast<FeedItem>().ToList();

        Assert.Single(feedItems);
        Assert.Equal("Full content body here", feedItems[0].Description);
    }

    [Fact]
    public async Task IngestAsync_HandlesAtomPublishedDateFallback()
    {
        var httpClient = CreateMockHttpClient(LoadFixture("atom-with-published.xml"));
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");
        var feedItems = items.Cast<FeedItem>().ToList();

        Assert.Equal(new DateTime(2024, 3, 1, 12, 0, 0), feedItems[0].PublishedDate.ToUniversalTime());
    }

    [Fact]
    public async Task IngestAsync_HandlesAtomMissingFields()
    {
        var httpClient = CreateMockHttpClient(LoadFixture("minimal-atom.xml"));
        var adapter = new FeedSourceAdapter(httpClient, NullLogger<FeedSourceAdapter>.Instance);

        var items = await adapter.IngestAsync("https://example.com/feed");
        var feedItems = items.Cast<FeedItem>().ToList();

        Assert.Single(feedItems);
        Assert.Equal("Title Only", feedItems[0].Title);
        Assert.Equal("", feedItems[0].Link);
        Assert.Equal("", feedItems[0].Description);
        Assert.Equal(DateTime.MinValue, feedItems[0].PublishedDate);
    }
}
