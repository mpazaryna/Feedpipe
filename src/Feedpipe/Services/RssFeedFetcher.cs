using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Feedpipe.Models;

namespace Feedpipe.Services;

public class RssFeedFetcher : IFeedFetcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RssFeedFetcher> _logger;

    public RssFeedFetcher(HttpClient httpClient, ILogger<RssFeedFetcher> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<FeedItem>> FetchAsync(string feedUrl)
    {
        _logger.LogInformation("Fetching feed: {Url}", feedUrl);

        try
        {
            var response = await _httpClient.GetStringAsync(feedUrl);
            var doc = XDocument.Parse(response);

            var items = doc.Descendants("item")
                .Select(item => new FeedItem(
                    Title: item.Element("title")?.Value ?? "(no title)",
                    Link: item.Element("link")?.Value ?? "",
                    Description: StripHtml(item.Element("description")?.Value ?? ""),
                    PublishedDate: DateTime.TryParse(item.Element("pubDate")?.Value, out var date)
                        ? date
                        : DateTime.MinValue
                ))
                .ToList();

            _logger.LogInformation("Parsed {Count} items from {Url}", items.Count, feedUrl);
            return items;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch feed: {Url}", feedUrl);
            return [];
        }
        catch (System.Xml.XmlException ex)
        {
            _logger.LogError(ex, "Failed to parse feed XML: {Url}", feedUrl);
            return [];
        }
    }

    private static string StripHtml(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", "").Trim();
    }
}
