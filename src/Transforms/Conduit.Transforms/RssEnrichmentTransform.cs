using System.Text.RegularExpressions;
using Conduit.Core.Models;
using Conduit.Core.Services;

namespace Conduit.Transforms;

/// <summary>
/// Enrichment stage that extracts keywords from RSS/Atom feed items.
/// </summary>
/// <remarks>
/// Extracts significant words from the title and description using
/// simple term frequency. Filters out common stop words and short tokens.
/// </remarks>
public partial class RssEnrichmentTransform : ITransform
{
    private static readonly HashSet<string> StopWords =
    [
        "the", "a", "an", "and", "or", "but", "in", "on", "at", "to",
        "for", "of", "with", "by", "from", "is", "are", "was", "were",
        "be", "been", "being", "have", "has", "had", "do", "does", "did",
        "will", "would", "could", "should", "may", "might", "can", "shall",
        "that", "this", "these", "those", "it", "its", "not", "no", "nor",
        "as", "if", "than", "too", "very", "just", "about", "above",
        "after", "before", "between", "into", "through", "during", "out",
        "up", "down", "over", "under", "again", "further", "then", "once",
        "here", "there", "when", "where", "why", "how", "all", "each",
        "every", "both", "few", "more", "most", "other", "some", "such",
        "only", "own", "same", "so", "also", "new"
    ];

    /// <inheritdoc />
    public Task<List<TransformedRecord<IPipelineRecord>>> ExecuteAsync(
        List<TransformedRecord<IPipelineRecord>> records)
    {
        foreach (var record in records)
        {
            if (record.Record is FeedItem feedItem)
            {
                record.Enrichment["keywords"] = ExtractKeywords(feedItem);
            }
        }

        return Task.FromResult(records);
    }

    private static List<string> ExtractKeywords(FeedItem item)
    {
        var text = $"{item.Title} {item.Description}".ToLowerInvariant();
        var words = WordPattern().Matches(text)
            .Select(m => m.Value)
            .Where(w => w.Length >= 3 && !StopWords.Contains(w))
            .GroupBy(w => w)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => g.Key)
            .ToList();

        return words;
    }

    [GeneratedRegex(@"[a-z][a-z0-9\-]+")]
    private static partial Regex WordPattern();
}
