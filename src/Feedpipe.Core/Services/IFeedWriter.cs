using Feedpipe.Core.Models;

namespace Feedpipe.Core.Services;

public interface IFeedWriter
{
    Task WriteAsync(List<FeedItem> items, string feedName);
}
