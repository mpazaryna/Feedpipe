using Feedpipe.Core.Models;

namespace Feedpipe.Core.Services;

public interface IFeedFetcher
{
    Task<List<FeedItem>> FetchAsync(string feedUrl);
}
