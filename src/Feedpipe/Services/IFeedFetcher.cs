using Feedpipe.Models;

namespace Feedpipe.Services;

public interface IFeedFetcher
{
    Task<List<FeedItem>> FetchAsync(string feedUrl);
}
