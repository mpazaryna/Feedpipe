namespace Conduit.Models;

/// <summary>
/// Configuration for a single RSS feed source.
/// </summary>
/// <remarks>
/// Bound from the <c>App:Feeds</c> array in <c>appsettings.json</c>.
/// Each entry maps to one feed that the pipeline will fetch.
///
/// <para>
/// The <c>required</c> keyword (C# 11+) tells the compiler that these
/// properties must be set during initialization. This catches missing
/// config values at startup rather than at runtime.
/// </para>
///
/// <para>
/// The <c>init</c> accessor means properties can only be set during
/// object initialization (e.g., by the config binder), not modified
/// afterward. This gives us practical immutability without needing a
/// full <c>record</c> type.
/// </para>
/// </remarks>
public class FeedSettings
{
    /// <summary>The URL of the RSS feed to fetch.</summary>
    public required string Url { get; init; }

    /// <summary>
    /// A short identifier for the feed (e.g., "hacker-news"). Used in
    /// output filenames and API routes.
    /// </summary>
    public required string Name { get; init; }
}

/// <summary>
/// Root configuration model for the Conduit application.
/// </summary>
/// <remarks>
/// <para>
/// Bound from the <c>App</c> section of <c>appsettings.json</c> using the
/// .NET Options pattern. In the console app, this is done manually via
/// <c>configuration.GetSection("App").Get&lt;AppSettings&gt;()</c>. In the
/// Worker and Api projects, it uses the more idiomatic
/// <c>services.Configure&lt;AppSettings&gt;()</c> which also supports
/// hot-reload when the config file changes.
/// </para>
///
/// <para>
/// Default values are specified inline so the app has sensible behavior
/// even if the config file is missing optional keys.
/// </para>
/// </remarks>
public class AppSettings
{
    /// <summary>Directory where fetched JSON files are written.</summary>
    public string OutputDir { get; init; } = "fetched";

    /// <summary>Directory where Serilog writes daily rolling log files.</summary>
    public string LogsDir { get; init; } = "logs";

    /// <summary>The list of RSS feeds to process.</summary>
    public List<FeedSettings> Feeds { get; init; } = [];
}
