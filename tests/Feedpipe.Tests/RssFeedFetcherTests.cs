using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Feedpipe.Services;
using Moq;
using Moq.Protected;

namespace Feedpipe.Tests;

public class RssFeedFetcherTests
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

    [Fact]
    public async Task FetchAsync_ParsesItemsFromRss()
    {
        var httpClient = CreateMockHttpClient(SampleRss);
        var fetcher = new RssFeedFetcher(httpClient, NullLogger<RssFeedFetcher>.Instance);

        var items = await fetcher.FetchAsync("https://example.com/feed");

        Assert.Equal(2, items.Count);
    }

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

    [Fact]
    public async Task FetchAsync_StripsHtmlFromDescription()
    {
        var httpClient = CreateMockHttpClient(SampleRss);
        var fetcher = new RssFeedFetcher(httpClient, NullLogger<RssFeedFetcher>.Instance);

        var items = await fetcher.FetchAsync("https://example.com/feed");

        Assert.Equal("Hello world", items[0].Description);
        Assert.Equal("No HTML here", items[1].Description);
    }

    [Fact]
    public async Task FetchAsync_ParsesPublishedDate()
    {
        var httpClient = CreateMockHttpClient(SampleRss);
        var fetcher = new RssFeedFetcher(httpClient, NullLogger<RssFeedFetcher>.Instance);

        var items = await fetcher.FetchAsync("https://example.com/feed");

        Assert.Equal(new DateTime(2024, 1, 1, 12, 0, 0), items[0].PublishedDate.ToUniversalTime());
    }

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
