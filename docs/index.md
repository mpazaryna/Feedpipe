---
_layout: landing
---

# Feedpipe

A production-ready data pipeline that fetches, parses, transforms, and serves content from multiple sources. Built with .NET 10.

## Projects

| Project | Description |
|---------|-------------|
| [Feedpipe.Core](api/Feedpipe.Core.Models.html) | Shared models and interfaces |
| [Feedpipe](api/Feedpipe.Services.html) | Console pipeline runner |
| [Feedpipe.Worker](api/Feedpipe.Worker.html) | Background service (timer-based) |
| [Feedpipe.Api](api/Feedpipe.Api.html) | REST API (ASP.NET minimal APIs) |
| [Feedpipe.Cli](api/Feedpipe.Cli.html) | Command-line tool |

## Quick Start

```bash
dotnet restore
dotnet run --project src/Feedpipe
dotnet test
```

## Documentation

- [API Reference](api/index.md) -- generated from XML doc comments
- [Roadmap](project/roadmap.md) -- project vision, milestones, and status
- [Decisions](project/adr/ADR-000-the-score.md) -- architecture decision records
- [Devlog](project/devlog/2026-Q1/2026-03-28-project-kickoff.md) -- development journal
