// -----------------------------------------------------------------------
// Conduit.Core - Shared domain models for the Conduit pipeline.
//
// This assembly contains the contracts (interfaces) and data models that
// all other projects in the solution depend on. Keeping these in a
// separate class library enforces a clean dependency graph:
//
//   Conduit.Core  <--  Conduit (console)
//                  <--  Conduit.Worker
//                  <--  Conduit.Api
//                  <--  Conduit.Cli
//                  <--  Conduit.Tests
//
// By isolating shared types here, we can swap implementations (e.g.,
// replace RssFeedFetcher with an AtomFeedFetcher) without touching
// any consuming project.
// -----------------------------------------------------------------------

namespace Conduit.Core.Models;

/// <summary>
/// Represents a single item parsed from an RSS or Atom feed.
/// </summary>
/// <remarks>
/// <para>
/// This is a C# <c>record</c> type, which gives us three things for free:
/// </para>
/// <list type="bullet">
///   <item><description>Immutability -- all properties are init-only.</description></item>
///   <item><description>Value equality -- two FeedItems with the same data are considered equal.</description></item>
///   <item><description>Built-in ToString() -- useful for debugging.</description></item>
/// </list>
/// <para>
/// Records are the .NET equivalent of Python's <c>@dataclass(frozen=True)</c>.
/// Use the <c>with</c> expression to create a modified copy:
/// <code>var updated = original with { Title = "New Title" };</code>
/// </para>
/// </remarks>
/// <param name="Title">The headline or title of the feed item.</param>
/// <param name="Link">The URL pointing to the full article or resource.</param>
/// <param name="Description">
/// A plain-text summary of the item. HTML tags are stripped during parsing.
/// </param>
/// <param name="PublishedDate">
/// When the item was published. Falls back to <see cref="DateTime.MinValue"/>
/// if the feed does not include a valid date.
/// </param>
public record FeedItem(
    string Title,
    string Link,
    string Description,
    DateTime PublishedDate
);
