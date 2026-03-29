using Conduit.Core.Models;

namespace Conduit.Core.Services;

/// <summary>
/// Defines the contract for fetching feed items from a remote source.
/// </summary>
/// <remarks>
/// <para>
/// This is an <b>interface</b> -- it declares <i>what</i> a feed fetcher does,
/// not <i>how</i> it does it. The actual implementation lives in a separate class
/// (e.g., <c>RssFeedFetcher</c>) that implements this interface.
/// </para>
/// <para>
/// Why use an interface instead of a concrete class?
/// </para>
/// <list type="number">
///   <item><description>
///     <b>Testability</b> -- tests can substitute a mock implementation that
///     returns canned data without hitting the network.
///   </description></item>
///   <item><description>
///     <b>Swappability</b> -- you can add an AtomFeedFetcher or an ApiFetcher
///     that implements the same interface, and swap them via DI config.
///   </description></item>
///   <item><description>
///     <b>Decoupling</b> -- consuming code (Worker, Api, Cli) depends on
///     the interface in Core, not the implementation in Conduit. This keeps
///     the dependency graph clean.
///   </description></item>
/// </list>
/// <para>
/// In Python you would achieve similar decoupling with abstract base classes
/// (ABC) or Protocol types. In .NET, interfaces are the standard approach
/// and are deeply integrated with the dependency injection system.
/// </para>
/// </remarks>
public interface IFeedFetcher
{
    /// <summary>
    /// Fetches and parses feed items from the given URL.
    /// </summary>
    /// <param name="feedUrl">
    /// The fully-qualified URL of the RSS or Atom feed to fetch.
    /// </param>
    /// <returns>
    /// A list of parsed <see cref="FeedItem"/> objects. Returns an empty list
    /// if the fetch fails or the feed contains no items.
    /// </returns>
    Task<List<FeedItem>> FetchAsync(string feedUrl);
}
