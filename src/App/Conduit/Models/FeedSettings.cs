namespace Conduit.Models;

/// <summary>
/// Configuration for a single data source.
/// </summary>
/// <remarks>
/// Bound from the <c>App:Sources</c> array in <c>appsettings.json</c>.
/// Each entry maps to one source that the pipeline will ingest.
/// </remarks>
public class SourceSettings
{
    /// <summary>
    /// The location of the source -- a URL for feeds, a file path for
    /// batch files, or any identifier the adapter understands.
    /// </summary>
    public required string Location { get; init; }

    /// <summary>
    /// A short identifier for the source (e.g., "hacker-news"). Used in
    /// output filenames and API routes.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The adapter type to use for this source (e.g., "rss", "atom", "edi834").
    /// Defaults to "rss" for backwards compatibility.
    /// </summary>
    public string Type { get; init; } = "rss";
}

/// <summary>
/// Root configuration model for the Conduit application.
/// </summary>
/// <remarks>
/// Bound from the <c>App</c> section of <c>appsettings.json</c> using the
/// .NET Options pattern.
/// </remarks>
public class AppSettings
{
    /// <summary>Directory where raw output JSON files are written (landing zone).</summary>
    public string OutputDir { get; init; } = "data/raw";

    /// <summary>Directory where curated (deduplicated, enriched) output is written.</summary>
    public string CuratedOutputDir { get; init; } = "data/curated";

    /// <summary>Directory where rejected (failed validation) records are written.</summary>
    public string RejectedOutputDir { get; init; } = "data/rejected";

    /// <summary>Directory where Serilog writes daily rolling log files.</summary>
    public string LogsDir { get; init; } = "logs";

    /// <summary>The list of data sources to process.</summary>
    public List<SourceSettings> Sources { get; init; } = [];
}
