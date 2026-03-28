namespace Feedpipe.Models;

public class FeedSettings
{
    public required string Url { get; init; }
    public required string Name { get; init; }
}

public class AppSettings
{
    public string OutputDir { get; init; } = "fetched";
    public string LogsDir { get; init; } = "logs";
    public List<FeedSettings> Feeds { get; init; } = [];
}
