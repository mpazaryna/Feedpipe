using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Feedpipe.Core.Models;
using Feedpipe.Core.Services;

namespace Feedpipe.Services;

/// <summary>
/// Fetches and parses RSS 2.0 feeds over HTTP.
/// </summary>
/// <remarks>
/// <para>
/// This is the concrete implementation of <see cref="IFeedFetcher"/>. It uses
/// <see cref="HttpClient"/> for HTTP requests and <see cref="XDocument"/> (LINQ to XML)
/// for parsing the RSS XML.
/// </para>
///
/// <para><b>Dependency Injection:</b></para>
/// <para>
/// Both dependencies (<c>HttpClient</c> and <c>ILogger</c>) are received through
/// the constructor. This is called <b>constructor injection</b> -- the DI container
/// creates the object and supplies its dependencies automatically. You never write
/// <c>new RssFeedFetcher(...)</c> in production code; the container does it for you.
/// </para>
///
/// <para><b>HttpClient lifecycle:</b></para>
/// <para>
/// The <c>HttpClient</c> is provided by <c>IHttpClientFactory</c> (registered via
/// <c>AddHttpClient&lt;IFeedFetcher, RssFeedFetcher&gt;()</c> in Program.cs). This is
/// important because creating <c>HttpClient</c> instances manually can lead to socket
/// exhaustion under load. The factory manages connection pooling and DNS refresh.
/// </para>
///
/// <para><b>Error handling strategy:</b></para>
/// <para>
/// Network errors (<see cref="HttpRequestException"/>) and malformed XML
/// (<see cref="System.Xml.XmlException"/>) are caught, logged, and converted to
/// empty results. This allows the pipeline to continue processing other feeds even
/// when one feed is down or returns invalid content.
/// </para>
/// </remarks>
public class RssFeedFetcher : IFeedFetcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RssFeedFetcher> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="RssFeedFetcher"/>.
    /// </summary>
    /// <param name="httpClient">
    /// The HTTP client used to fetch feed content. Managed by IHttpClientFactory.
    /// </param>
    /// <param name="logger">
    /// Typed logger for structured logging. The <c>&lt;RssFeedFetcher&gt;</c> type
    /// parameter means log entries automatically include this class name as the
    /// source context, so you can filter logs by component.
    /// </param>
    public RssFeedFetcher(HttpClient httpClient, ILogger<RssFeedFetcher> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// The parsing logic uses LINQ to XML to query all <c>&lt;item&gt;</c> elements
    /// in the RSS document. The <c>?.</c> (null-conditional) and <c>??</c>
    /// (null-coalescing) operators provide safe fallbacks for missing elements,
    /// which is common in real-world RSS feeds.
    /// </remarks>
    public async Task<List<FeedItem>> FetchAsync(string feedUrl)
    {
        _logger.LogInformation("Fetching feed: {Url}", feedUrl);

        try
        {
            var response = await _httpClient.GetStringAsync(feedUrl);
            var doc = XDocument.Parse(response);

            // LINQ query: find all <item> elements anywhere in the document,
            // project each one into a FeedItem record using named arguments.
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

    /// <summary>
    /// Removes HTML tags from a string, leaving only plain text.
    /// </summary>
    /// <param name="html">A string that may contain HTML markup.</param>
    /// <returns>The input string with all HTML tags removed and whitespace trimmed.</returns>
    private static string StripHtml(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", "").Trim();
    }
}
