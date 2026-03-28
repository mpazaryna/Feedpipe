# Feedpipe

A data pipeline that fetches, parses, and stores content from RSS feeds. Built with .NET 10.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Getting Started

```bash
# Restore dependencies
dotnet restore

# Run the pipeline
dotnet run --project src/Feedpipe

# Run tests
dotnet test
```

## Configuration

Feed sources and output paths are configured in `src/Feedpipe/appsettings.json`:

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
Feedpipe.slnx                       # Solution file
Directory.Build.props                # Shared build settings (warnings, analysis)
.editorconfig                        # C# coding style enforcement
src/
  Feedpipe/
    Program.cs                       # Entry point, DI container, logging setup
    appsettings.json                 # Configuration
    Models/
      FeedItem.cs                    # Immutable data model (record type)
      FeedSettings.cs                # Configuration models
    Services/
      IFeedFetcher.cs                # Interface for fetching feeds
      RssFeedFetcher.cs              # RSS/XML feed fetcher implementation
      IFeedWriter.cs                 # Interface for writing feed data
      JsonFeedWriter.cs              # JSON file writer implementation
tests/
  Feedpipe.Tests/
    RssFeedFetcherTests.cs           # Feed parsing tests (mocked HTTP)
    JsonFeedWriterTests.cs           # File writing tests (temp directory)
    GlobalUsings.cs                  # Shared test imports
```

## Output

- **fetched/** - JSON files with parsed feed items
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
