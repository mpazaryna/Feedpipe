# Conduit

[![Docs](https://img.shields.io/badge/docs-GitHub%20Pages-blue)](https://mpazaryna.github.io/Conduit/)
[![CI](https://github.com/mpazaryna/Conduit/actions/workflows/ci.yml/badge.svg)](https://github.com/mpazaryna/Conduit/actions/workflows/ci.yml)
[![Tests](https://img.shields.io/badge/tests-61%20passing-green)](https://github.com/mpazaryna/Conduit/actions/workflows/ci.yml)
[![Coverage](https://img.shields.io/badge/coverage-97%25-brightgreen)](https://github.com/mpazaryna/Conduit/actions/workflows/ci.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/download)

A domain-agnostic data pipeline that ingests, transforms, and serves data from multiple source types. Built with .NET 10.

## Source Adapters

| Adapter | Format | Pattern |
|---------|--------|---------|
| RSS / Atom | XML feeds | Fetch from URL, auto-detect format |
| EDI 834 | Healthcare enrollment | Read local file, parse X12 segments |
| Zotero | Research library CSV | Read local file, enrich from arxiv API |

## Getting Started

```bash
dotnet restore
dotnet run --project src/App/Conduit
dotnet test
```

## Configuration

Sources are configured in `src/App/Conduit/appsettings.json`:

```json
{
  "App": {
    "OutputDir": "data",
    "Sources": [
      { "Location": "https://hnrss.org/frontpage", "Name": "hacker-news", "Type": "rss" },
      { "Location": "samples/edi834/sample-834.edi", "Name": "benefits-enrollment", "Type": "edi834" },
      { "Location": "samples/zotero/paz-zotero.csv", "Name": "research-papers", "Type": "zotero" }
    ]
  }
}
```

## Project Structure

```
src/
  Core/
    Conduit.Core/                    Shared interfaces and models
  Adapters/
    Conduit.Sources.Rss/             RSS/Atom feed adapter
    Conduit.Sources.Edi834/          EDI 834 healthcare adapter
    Conduit.Sources.Zotero/          Zotero CSV + arxiv adapter
  App/
    Conduit/                         Console pipeline runner
    Conduit.Worker/                  Background service (5 min schedule)
    Conduit.Api/                     REST API (ASP.NET minimal APIs)
    Conduit.Cli/                     CLI tool (search, list, stats)
tests/
  Conduit.Tests/                     Output writer tests
  Adapters/                          Adapter-specific tests
data/                                Pipeline output (by source type)
samples/                             Input test data
```

## How to Run

```bash
# One-shot pipeline (all sources)
dotnet run --project src/App/Conduit

# Background service (every 5 minutes)
dotnet run --project src/App/Conduit.Worker

# REST API
dotnet run --project src/App/Conduit.Api

# CLI
dotnet run --project src/App/Conduit.Cli -- list
dotnet run --project src/App/Conduit.Cli -- search "AI"
dotnet run --project src/App/Conduit.Cli -- stats
```

## Key Patterns

- **Adapter pattern** -- pluggable source types via `ISourceAdapter`
- **Keyed DI services** -- runtime adapter resolution from config
- **Concurrent processing** -- sources ingested in parallel via `Task.WhenAll`
- **IPipelineRecord** -- domain-agnostic base type for all pipeline records
- **Structured logging** -- Serilog with console and file sinks
- **TDD** -- 61 tests across 4 projects, 97% coverage reported in CI

## Tech Stack

- .NET 10
- Serilog (logging)
- xUnit + Moq (testing)
- coverlet (code coverage)
- GitHub Actions (CI)

## Documentation

All documentation lives in the repo ([ADR-003](.orchestra/adr/ADR-003-no-docs-site.md)).

- [Roadmap](.orchestra/roadmap.md) -- project vision and milestone status
- [Decisions](.orchestra/adr/) -- architecture decision records
- [Devlog](.orchestra/devlog/2026-Q1/) -- development journal and learning notes
- XML doc comments in every `.cs` file -- renders as IntelliSense in your IDE
