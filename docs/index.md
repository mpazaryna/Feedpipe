---
_layout: landing
---

# Feedpipe API Reference

A production-ready data pipeline that fetches, parses, transforms, and serves content from multiple sources. Built with .NET 10.

## Namespaces

| Namespace | Description |
|-----------|-------------|
| [Feedpipe.Core.Models](api/Feedpipe.Core.Models.html) | Shared data models (FeedItem) |
| [Feedpipe.Core.Services](api/Feedpipe.Core.Services.html) | Service contracts (IFeedFetcher, IFeedWriter) |
| [Feedpipe.Services](api/Feedpipe.Services.html) | Implementations (RssFeedFetcher, JsonFeedWriter) |
| [Feedpipe.Models](api/Feedpipe.Models.html) | Application configuration (AppSettings, FeedSettings) |
| [Feedpipe.Worker](api/Feedpipe.Worker.html) | Background service for scheduled pipeline runs |

## Quick Start

```bash
dotnet restore
dotnet run --project src/Feedpipe
dotnet test
```
