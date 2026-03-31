using System.Text.RegularExpressions;
using Conduit.Core.Models;
using Conduit.Core.Services;
using Conduit.Sources.Zotero.Models;

namespace Conduit.Transforms;

/// <summary>
/// Validates Zotero research records against metadata quality rules.
/// </summary>
public partial class ResearchRecordValidator : IRecordValidator
{
    /// <inheritdoc />
    public bool AppliesTo(IPipelineRecord record) => record is ResearchRecord;

    /// <inheritdoc />
    public IReadOnlyList<string> Validate(IPipelineRecord record)
    {
        var research = (ResearchRecord)record;
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(research.Title))
            errors.Add("Title is required");

        if (string.IsNullOrWhiteSpace(research.Doi) && string.IsNullOrWhiteSpace(research.Url))
            errors.Add("At least one identifier (Doi or Url) is required");

        if (!string.IsNullOrWhiteSpace(research.Doi) && !DoiPattern().IsMatch(research.Doi))
            errors.Add($"DOI '{research.Doi}' does not match the expected format (10.xxxx/...)");

        if (string.IsNullOrWhiteSpace(research.Abstract) && research.AccessLevel == AccessLevel.Open)
            errors.Add("Abstract is empty for an Open access record — may indicate incomplete metadata");

        return errors;
    }

    [GeneratedRegex(@"^10\.\d{4,}/.+")]
    private static partial Regex DoiPattern();
}
