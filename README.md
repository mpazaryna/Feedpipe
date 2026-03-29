# Conduit

A production-ready data pipeline that ingests, transforms, and serves data from multiple source types. Built with .NET 10.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Getting Started

```bash
# Restore dependencies
dotnet restore

# Run the pipeline
dotnet run --project src/Conduit

# Run tests
dotnet test
```

## Configuration

Sources and output paths are configured in `src/Conduit/appsettings.json`:

```json
{
  "App": {
    "OutputDir": "fetched",
    "LogsDir": "logs",
    "Feeds": [
      { "Url": "https://hnrss.org/frontpage", "Name": "hacker-news" }
    ]
  }
}
```

## Project Structure

```
Conduit.slnx                        # Solution file
Directory.Build.props                # Shared build settings (warnings, analysis)
.editorconfig                        # C# coding style enforcement
src/
  Conduit.Core/                      # Shared models and interfaces
  Conduit/                           # Console pipeline runner
  Conduit.Worker/                    # Background service (timer-based)
  Conduit.Api/                       # REST API (ASP.NET minimal APIs)
  Conduit.Cli/                       # Command-line tool
tests/
  Conduit.Tests/                     # xUnit tests with mocked HTTP
```

## Output

- **fetched/** - JSON files with parsed data
- **logs/** - Daily rolling log files via Serilog

## Key Patterns

- **Dependency Injection** - Services registered in a DI container, resolved at runtime
- **Interface + Implementation** - Services defined by contract, swappable
- **appsettings.json** - Externalized configuration bound to typed settings classes
- **ILogger\<T\>** - Typed, structured logging via Serilog with console and file sinks
- **Record types** - Immutable data models with value equality
- **Error handling** - Network and XML parse failures are logged and recovered from
- **Code analysis** - TreatWarningsAsErrors + AnalysisLevel enforced via Directory.Build.props

## Tech Stack

- .NET 10
- Serilog (logging)
- xUnit (testing)
- Moq (mocking)
