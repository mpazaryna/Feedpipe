# 2026-03-29: Source Adapter Isolation

## What Happened

Extracted the RSS adapter out of the main Conduit project into its own assembly (`Conduit.Sources.Rss`). Mirrored the same structure in tests (`Conduit.Sources.Rss.Tests`). This sets the pattern for all future source adapters.

Also completed the generalization refactor that preceded this:
- `IFeedFetcher` -> `ISourceAdapter` (method: `IngestAsync`, param: `location`)
- `IFeedWriter` -> `IOutputWriter`
- `FeedSettings` -> `SourceSettings` with a `Type` field
- Config: `Feeds` array -> `Sources` array
- API routes: `/feeds` -> `/sources`

## Why

Each source type will carry its own dependencies. An 834 adapter needs an X12 parser. An Arxiv adapter might need a PDF library or REST client. Keeping them in the same project means every deployment ships every dependency regardless of which sources are configured. Separate assemblies mean each source is self-contained.

## Structure After

```
src/
  Conduit.Core/                    Interfaces + models (zero deps)
  Conduit/                         Pipeline, config, DI, output writers
  Conduit.Sources.Rss/             RSS adapter (own assembly, own deps)
tests/
  Conduit.Tests/                   Output writer tests (4)
  Conduit.Sources.Rss.Tests/       RSS adapter tests (6)
```

Adding a new source: create `Conduit.Sources.{Name}` and `Conduit.Sources.{Name}.Tests`. Reference `Conduit.Core`. No existing code changes.

## What's Next

The codebase is now ready for the Multi-Source Ingestion milestone. The abstraction layer exists (`ISourceAdapter`, `IOutputWriter`, `SourceSettings` with `Type`), and the project structure supports isolated source adapters. Next step is writing the spec and starting implementation.
