---
sidebar_position: 1
slug: /
---

# Feedpipe

A production-ready data pipeline that fetches, parses, transforms, and serves content from multiple sources. Built with .NET 10.

## Projects

| Project | Description |
|---------|-------------|
| **Feedpipe.Core** | Shared models and interfaces |
| **Feedpipe** | Console pipeline runner |
| **Feedpipe.Worker** | Background service (timer-based) |
| **Feedpipe.Api** | REST API (ASP.NET minimal APIs) |
| **Feedpipe.Cli** | Command-line tool |

## Quick Start

```bash
dotnet restore
dotnet run --project src/Feedpipe
dotnet test
```

## How to Run Each Project

```bash
# One-shot pipeline
dotnet run --project src/Feedpipe

# Background service (fetches every 5 minutes)
dotnet run --project src/Feedpipe.Worker

# REST API
dotnet run --project src/Feedpipe.Api

# CLI tool
dotnet run --project src/Feedpipe.Cli -- list
dotnet run --project src/Feedpipe.Cli -- search "AI"
dotnet run --project src/Feedpipe.Cli -- stats
```

## Key Patterns

- **Dependency Injection** -- services registered in a DI container, resolved at runtime
- **Interface + Implementation** -- services defined by contract, swappable
- **appsettings.json** -- externalized configuration bound to typed settings classes
- **ILogger\<T\>** -- typed, structured logging via Serilog with console and file sinks
- **Record types** -- immutable data models with value equality
- **Error handling** -- network and XML parse failures are logged and recovered from

## Tech Stack

- .NET 10
- Serilog (logging)
- xUnit (testing)
- Moq (mocking)
