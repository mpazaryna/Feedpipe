# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

```bash
# Build
dotnet build --configuration Release

# Run all tests with coverage
dotnet test --configuration Release --verbosity normal \
  --collect:"XPlat Code Coverage" \
  --results-directory:"./TestResults" \
  --settings coverage.runsettings

# Run a single test project
dotnet test tests/Adapters/Conduit.Sources.Rss.Tests

# Run a single test by name
dotnet test --filter "FullyQualifiedName~IngestAsync_ParsesItemsFromRss"
```

## Running the Application

Four entry points, each in `src/App/`:

```bash
dotnet run --project src/App/Conduit              # One-shot console pipeline
dotnet run --project src/App/Conduit.Worker        # Background service (5-min schedule)
dotnet run --project src/App/Conduit.Api            # REST API on localhost:5000
dotnet run --project src/App/Conduit.Cli -- list    # CLI (list, search, stats)
```

## Architecture

Conduit is a domain-agnostic data pipeline that ingests from heterogeneous sources (RSS/Atom, EDI 834, Zotero), transforms records (dedup, enrichment), and writes output to a raw/curated file structure.

### Pipeline Flow

```
ISourceAdapter.IngestAsync()
  → List<IPipelineRecord>
  → JsonOutputWriter           → data/raw/{sourceType}/
  → TransformPipeline
    → DeduplicationTransform   (filters duplicates within and across runs)
    → *EnrichmentTransform     (adds derived fields per source type)
  → JsonTransformedOutputWriter → data/curated/{sourceType}/
```

### Project Layout

```
src/Core/Conduit.Core/              # Interfaces: IPipelineRecord, ISourceAdapter, ITransform
src/Adapters/Conduit.Sources.Rss/   # RSS 2.0 & Atom feed parser (auto-detects format)
src/Adapters/Conduit.Sources.Edi834/# EDI 834 X12 healthcare enrollment parser
src/Adapters/Conduit.Sources.Zotero/# Zotero CSV parser + arxiv API enrichment
src/Transforms/Conduit.Transforms/  # Enrichment transforms (RSS keywords, EDI status, Zotero domains)
src/App/Conduit/                    # Console runner + shared DI (AddConduitPipeline)
src/App/Conduit.Worker/             # BackgroundService on timer
src/App/Conduit.Api/                # ASP.NET Minimal APIs
src/App/Conduit.Cli/                # CLI tool (reads from data/curated/)
tests/                              # xUnit + Moq, fixture-based (107 tests)
```

### Adding a New Source Adapter

1. Create `src/Adapters/Conduit.Sources.{Name}/` implementing `ISourceAdapter`
2. Create a domain model implementing `IPipelineRecord`
3. Register in `ServiceCollectionExtensions.AddConduitPipeline()`
4. Add a source entry in `appsettings.json` with matching `Type` key

### Adding a New Transform

1. Implement `ITransform` in `src/Transforms/Conduit.Transforms/`
2. Register in `ServiceCollectionExtensions.AddConduitPipeline()`

## Project Tracking

Tasks are tracked in ClickUp (list ID `901712385324`). The ClickUp API key is in `.env` — use the API to read/write tasks.

## Documentation

Project docs (roadmap, ADRs, devlog) live in `.orchestra/` and deploy to GitHub Pages via Docusaurus (`docs/`). See ADR-003.
