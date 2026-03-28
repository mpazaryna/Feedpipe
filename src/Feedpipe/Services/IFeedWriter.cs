using Feedpipe.Models;

namespace Feedpipe.Services;

public interface IFeedWriter
{
    Task WriteAsync(List<FeedItem> items, string feedName);
}
