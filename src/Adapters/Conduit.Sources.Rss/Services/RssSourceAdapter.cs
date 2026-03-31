using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Conduit.Core.Models;
using Conduit.Core.Services;

namespace Conduit.Sources.Rss.Services;

/// <summary>
/// Ingests and parses RSS 2.0 and Atom feeds over HTTP with automatic
/// format detection.
/// </summary>
/// <remarks>
/// <para>
/// Auto-detects the feed format by inspecting the root XML element:
/// <c>&lt;rss&gt;</c> for RSS 2.0, <c>&lt;feed&gt;</c> for Atom.
/// The caller does not need to specify which format the URL returns.
/// </para>
///
/// <para><b>Error handling:</b></para>
/// <para>
/// Network errors, malformed XML, and unrecognized formats are caught,
/// logged, and converted to empty results. The pipeline continues
/// processing other sources.
/// </para>
/// </remarks>
public class FeedSourceAdapter : ISourceAdapter
{
    private static readonly XNamespace AtomNs = "http://www.w3.org/2005/Atom";

    private readonly HttpClient _httpClient;
    private readonly ILogger<FeedSourceAdapter> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="FeedSourceAdapter"/>.
    /// </summary>
    /// <param name="httpClient">HTTP client managed by IHttpClientFactory.</param>
    /// <param name="logger">Typed logger for structured logging.</param>
    public FeedSourceAdapter(HttpClient httpClient, ILogger<FeedSourceAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<IPipelineRecord>> IngestAsync(string location)
    {
        _logger.LogInformation("Ingesting feed source: {Location}", location);

        try
        {
            // `await` suspends this method until the HTTP response arrives,
            // freeing the thread to do other work while the network call is in flight.
            var response = await _httpClient.GetStringAsync(location);
            var doc = XDocument.Parse(response);

            // `?.` is the null-conditional operator — if `doc.Root` is null, the
            // whole expression short-circuits to null instead of throwing NullReferenceException.
            var rootName = doc.Root?.Name.LocalName;

            // Switch expression (C# 8+) — more concise than a switch statement.
            // Each arm is `pattern => expression`; `_` is the discard / default arm.
            var items = rootName switch
            {
                "rss" => ParseRss(doc),
                "feed" => ParseAtom(doc),
                _ => HandleUnknownFormat(rootName, location)
            };

            _logger.LogInformation("Parsed {Count} items from {Location}", items.Count, location);
            return items;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch source: {Location}", location);
            return []; // Collection expression (C# 12) — equivalent to new List<IPipelineRecord>()
        }
        catch (System.Xml.XmlException ex)
        {
            _logger.LogError(ex, "Failed to parse source XML: {Location}", location);
            return [];
        }
    }

    private static List<IPipelineRecord> ParseRss(XDocument doc)
    {
        // `Descendants("item")` finds all <item> elements anywhere in the XML tree.
        // `.Select()` maps each XML element to a FeedItem using named argument syntax.
        // `?.Value ?? fallback` chains two null-handling operators:
        //   `?.` — returns null if the element doesn't exist instead of throwing
        //   `??` — substitutes the fallback value when the left side is null
        // The cast `(IPipelineRecord)` is needed because Select infers List<FeedItem>
        // but the return type declares List<IPipelineRecord>.
        return doc.Descendants("item")
            .Select(item => (IPipelineRecord)new FeedItem(
                Title: item.Element("title")?.Value ?? "(no title)",
                Link: item.Element("link")?.Value ?? "",
                Description: StripHtml(item.Element("description")?.Value ?? ""),
                PublishedDate: DateTime.TryParse(item.Element("pubDate")?.Value, out var date)
                    ? date
                    : DateTime.MinValue
            ))
            .ToList();
    }

    private static List<IPipelineRecord> ParseAtom(XDocument doc)
    {
        // Atom uses XML namespaces — element names must be qualified with the namespace.
        // `AtomNs + "entry"` uses XNamespace's `+` operator overload to produce an
        // XName like `{http://www.w3.org/2005/Atom}entry`. Without the namespace,
        // `doc.Descendants("entry")` finds nothing because the names don't match.
        return doc.Descendants(AtomNs + "entry")
            .Select(entry => (IPipelineRecord)new FeedItem(
                Title: entry.Element(AtomNs + "title")?.Value ?? "(no title)",
                Link: entry.Element(AtomNs + "link")?.Attribute("href")?.Value ?? "",
                Description: StripHtml(entry.Element(AtomNs + "summary")?.Value
                    ?? entry.Element(AtomNs + "content")?.Value ?? ""),
                PublishedDate: DateTime.TryParse(
                    entry.Element(AtomNs + "updated")?.Value
                    ?? entry.Element(AtomNs + "published")?.Value, out var date)
                    ? date
                    : DateTime.MinValue
            ))
            .ToList();
    }

    private List<IPipelineRecord> HandleUnknownFormat(string? rootName, string location)
    {
        _logger.LogWarning("Unrecognized feed format '{RootElement}' from {Location}", rootName, location);
        return [];
    }

    private static string StripHtml(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", "").Trim();
    }
}
