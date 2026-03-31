using Conduit.Core.Models;
using Conduit.Core.Services;

namespace Conduit.Transforms;

/// <summary>
/// Validates RSS/Atom feed items against content quality rules.
/// </summary>
public class FeedItemValidator : IRecordValidator
{
    /// <inheritdoc />
    public bool AppliesTo(IPipelineRecord record) => record is FeedItem;

    /// <inheritdoc />
    public IReadOnlyList<string> Validate(IPipelineRecord record)
    {
        var item = (FeedItem)record;
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(item.Title))
            errors.Add("Title is required");

        if (string.IsNullOrWhiteSpace(item.Link))
        {
            errors.Add("Link is required");
        }
        else if (!Uri.TryCreate(item.Link, UriKind.Absolute, out var uri) ||
                 (uri.Scheme != "http" && uri.Scheme != "https"))
        {
            errors.Add($"Link '{item.Link}' is not a valid absolute URL");
        }

        if (item.PublishedDate == DateTime.MinValue)
            errors.Add("PublishedDate is missing (DateTime.MinValue indicates no date was parsed from the feed)");

        return errors;
    }
}
