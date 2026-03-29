---
_layout: landing
---

# Conduit API Reference

A production-ready data pipeline that ingests, transforms, and serves data from multiple source types. Built with .NET 10.

## Namespaces

| Namespace | Description |
|-----------|-------------|
| [Conduit.Core.Models](api/Conduit.Core.Models.html) | Shared data models (FeedItem) |
| [Conduit.Core.Services](api/Conduit.Core.Services.html) | Service contracts (IFeedFetcher, IFeedWriter) |
| [Conduit.Services](api/Conduit.Services.html) | Implementations (RssFeedFetcher, JsonFeedWriter) |
| [Conduit.Models](api/Conduit.Models.html) | Application configuration (AppSettings, FeedSettings) |
| [Conduit.Worker](api/Conduit.Worker.html) | Background service for scheduled pipeline runs |

## Quick Start

```bash
dotnet restore
dotnet run --project src/Conduit
dotnet test
```
