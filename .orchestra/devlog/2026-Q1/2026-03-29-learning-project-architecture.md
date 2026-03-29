# Learning: .NET Project Architecture

How Conduit is structured and why, with patterns you'll see on any .NET team.

## Solution Layout

```
Conduit/
  Conduit.slnx                    <- Solution file (groups everything)
  Directory.Build.props            <- Shared build settings
  .editorconfig                    <- Coding style rules
  src/
    Core/
      Conduit.Core/                <- Shared interfaces + models (zero deps)
    Adapters/
      Conduit.Sources.Rss/         <- RSS/Atom adapter
      Conduit.Sources.Edi834/      <- Healthcare 834 adapter
      Conduit.Sources.Zotero/      <- Zotero/arxiv adapter
    App/
      Conduit/                     <- Console runner + DI wiring
      Conduit.Worker/              <- Background service
      Conduit.Api/                 <- REST API
      Conduit.Cli/                 <- CLI tool
  tests/
    Conduit.Tests/                 <- Output writer tests
    Adapters/                      <- Mirror of src/Adapters
  data/                            <- Pipeline output (by adapter type)
  samples/                         <- Input test data
  .orchestra/                      <- Project management docs
```

## The Dependency Graph

```
Conduit.Core  <--  Conduit.Sources.Rss
              <--  Conduit.Sources.Edi834
              <--  Conduit.Sources.Zotero
              <--  Conduit (App)  <--  Conduit.Worker
                                  <--  Conduit.Api
              <--  Conduit.Cli
```

The rule: dependencies flow inward toward Core. Core depends on nothing. Adapters depend only on Core. App projects depend on Core and the adapters they need.

This means you can delete `Conduit.Sources.Edi834` entirely and nothing else breaks (except the DI registration in Program.cs).

## Why Separate Projects Instead of Folders

Each `.csproj` is a separate assembly (DLL). This gives you:
- **Isolated dependencies** -- the 834 adapter can use an X12 library without RSS pulling it in
- **Independent compilation** -- changing an adapter only recompiles that adapter and its dependents
- **Clear boundaries** -- if a class needs a `using` for another project, it must declare a `<ProjectReference>`

In Python, you'd achieve similar separation with separate packages in a monorepo. The difference is that .NET enforces it at compile time.

## Four Entry Points, Same Pipeline

Conduit has four ways to run the same pipeline logic:

| Project | Purpose | How it runs |
|---------|---------|-------------|
| Conduit (console) | One-shot pipeline run | `dotnet run --project src/App/Conduit` |
| Conduit.Worker | Scheduled background service | Runs continuously, fetches every 5 min |
| Conduit.Api | HTTP API | REST endpoints to trigger and query |
| Conduit.Cli | Query tool | Search, list, stats on stored data |

All four share the same Core interfaces, adapters, and output writer. They differ only in how they're triggered and how they present results. This is a common .NET pattern -- the business logic lives in libraries, and multiple "hosts" consume it.

## Configuration Pattern

```json
{
  "App": {
    "OutputDir": "data",
    "Sources": [
      { "Location": "https://hnrss.org/frontpage", "Name": "hacker-news", "Type": "rss" },
      { "Location": "samples/edi834/sample-834.edi", "Name": "benefits", "Type": "edi834" }
    ]
  }
}
```

Configuration is loaded from `appsettings.json` and bound to typed C# classes (`AppSettings`, `SourceSettings`). This means:
- Config errors are caught at startup (wrong type, missing required field)
- Code accesses `settings.Sources` not `config["App:Sources"]` -- type-safe
- The Options pattern (`IOptions<AppSettings>`) supports hot-reload in Worker/Api

## Logging Pattern

```csharp
_logger.LogInformation("Parsed {Count} items from {Location}", items.Count, location);
```

Key things:
- `{Count}` and `{Location}` are **named placeholders**, not string interpolation. Log aggregators (Datadog, Application Insights) can index and query by these fields.
- `ILogger<T>` is typed -- each class gets its own logger that includes the class name as source context. You can filter logs by component.
- Serilog handles output. Console sink for development, file sink for audit. The application code doesn't know or care where logs go.

## The Adapter Pattern

Every source adapter implements one interface:

```csharp
public interface ISourceAdapter
{
    Task<List<IPipelineRecord>> IngestAsync(string location);
}
```

The pipeline calls `IngestAsync` and gets records back. It doesn't know if the adapter is fetching from a URL, reading a file, calling an API, or all three (like Zotero does).

Adding a new source type:
1. Create `Conduit.Sources.{Name}` project
2. Implement `ISourceAdapter`
3. Define a domain record type implementing `IPipelineRecord`
4. Register in DI with a key
5. Add to `appsettings.json`

No changes to the pipeline, no changes to existing adapters, no changes to tests for other adapters. This is Open/Closed in practice.
