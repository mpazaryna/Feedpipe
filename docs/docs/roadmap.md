---
sidebar_position: 2
---

# Roadmap

**Objective:** A production-ready .NET data pipeline that fetches, transforms, and serves content from multiple sources with pluggable adapters, transformation stages, and resilient delivery.

## Success Criteria

- Multi-source ingestion beyond RSS (Atom, REST APIs)
- Data transformation and enrichment layer
- Production-grade resilience (retries, health checks, monitoring)
- Deployable as a container to Azure or any Linux host
- Comprehensive test coverage and documentation

## Context

Feedpipe is a content aggregation and data pipeline platform built on .NET 10. It ingests content from diverse sources, transforms and enriches it, and serves it through multiple interfaces. The architecture follows a staged pipeline: fetch -> parse -> transform -> store, with multiple entry points (console runner, background worker, REST API, CLI).

The pipeline pattern is domain-agnostic -- applicable to news aggregation, research monitoring, competitive intelligence, healthcare data ingestion, or any scenario where structured content needs to be collected from heterogeneous sources.

## Milestones

| Milestone | Status |
|-----------|--------|
| [Foundation](milestones/foundation.md) | **Done** |
| [Multi-Source Ingestion](milestones/multi-source-ingestion.md) | Not Started |
| [Data Transformation](milestones/data-transformation.md) | Not Started |
| [Production Hardening](milestones/production-hardening.md) | Not Started |
