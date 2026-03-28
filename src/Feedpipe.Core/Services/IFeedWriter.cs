using Feedpipe.Core.Models;

namespace Feedpipe.Core.Services;

/// <summary>
/// Defines the contract for persisting feed items to a storage backend.
/// </summary>
/// <remarks>
/// <para>
/// The current implementation (<c>JsonFeedWriter</c>) writes to the local
/// filesystem as JSON files. However, because consuming code depends on this
/// interface rather than the concrete class, you could create alternative
/// implementations that write to a database, blob storage, or message queue
/// without changing any calling code.
/// </para>
/// <para>
/// This is the <b>Strategy pattern</b> in action -- the "how" of persistence
/// is a pluggable strategy, selected at DI registration time.
/// </para>
/// </remarks>
public interface IFeedWriter
{
    /// <summary>
    /// Persists a collection of feed items under the given feed name.
    /// </summary>
    /// <param name="items">The feed items to persist.</param>
    /// <param name="feedName">
    /// A short identifier for the feed (e.g., "hacker-news"). Used to
    /// organize output -- in the JSON implementation, this becomes part
    /// of the filename.
    /// </param>
    /// <returns>A task that completes when the write operation finishes.</returns>
    Task WriteAsync(List<FeedItem> items, string feedName);
}
