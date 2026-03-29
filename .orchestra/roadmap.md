# Conduit Roadmap

**Objective:** A production-ready .NET data pipeline that fetches, transforms, and serves content from multiple sources with pluggable adapters, transformation stages, and resilient delivery.

## Success Criteria

- [ ] Multi-source ingestion beyond RSS (Atom, EDI 834)
- [ ] Data transformation and enrichment layer
- [ ] Production-grade resilience (retries, health checks, monitoring)
- [ ] Deployable as a container to Azure or any Linux host
- [ ] Comprehensive test coverage and documentation

## Context

Conduit is a content aggregation and data pipeline platform built on .NET 10. It ingests content from diverse sources, transforms and enriches it, and serves it through multiple interfaces. The architecture follows a staged pipeline: fetch -> parse -> transform -> store, with multiple entry points (console runner, background worker, REST API, CLI).

The pipeline pattern is domain-agnostic -- applicable to news aggregation, research monitoring, competitive intelligence, healthcare data ingestion, or any scenario where structured content needs to be collected from heterogeneous sources.

## Milestones

### Foundation

Establishes the core project structure, DI, logging, testing, CI/CD, and documentation. Provides four entry points (console, worker, API, CLI) and a clean architecture that all subsequent milestones build on.

- PRD: [.orchestra/work/foundation/prd.md](.orchestra/work/foundation/prd.md)
- Dependency: None
- Status: Complete

### Multi-Source Ingestion

Introduces a pluggable source adapter pattern so the pipeline can ingest data from any structured source -- RSS, Atom, and EDI 834 healthcare enrollment files. Proves the architecture works across content feeds and transactional healthcare data.

- PRD: [.orchestra/work/multi-source-ingestion/prd.md](.orchestra/work/multi-source-ingestion/prd.md)
- Dependency: Foundation
- Status: Not Started

### Data Transformation

Adds a composable transformation layer between ingestion and storage. Handles deduplication, content enrichment, validation, and pluggable storage backends. Turns Conduit from a data fetcher into a data pipeline.

- PRD: [.orchestra/work/data-transformation/prd.md](.orchestra/work/data-transformation/prd.md)
- Dependency: Multi-Source Ingestion
- Status: Not Started

### Production Hardening

Makes the pipeline reliable enough to run unattended in production. Adds retry policies, health checks, observability, containerized deployment, and graceful shutdown. Closes the gap between "it works" and "it runs."

- PRD: [.orchestra/work/production-hardening/prd.md](.orchestra/work/production-hardening/prd.md)
- Dependency: Data Transformation
- Status: Not Started

## References

- ADR-000: [The Score](adr/ADR-000-the-score.md)
