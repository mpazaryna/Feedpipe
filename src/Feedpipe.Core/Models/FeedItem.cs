namespace Feedpipe.Core.Models;

public record FeedItem(
    string Title,
    string Link,
    string Description,
    DateTime PublishedDate
);
