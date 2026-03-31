# Conduit Roadmap

**Objective:** A production-ready .NET data pipeline that fetches, transforms, and serves content from multiple sources with pluggable adapters, transformation stages, and resilient delivery.

## Success Criteria

- [x] Multi-source ingestion beyond RSS (Atom, EDI 834, Zotero)
- [x] Data transformation and enrichment layer
- [x] Validation and rejected data tier
- [ ] Deep domain coverage across all three source types
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

Introduces a pluggable source adapter pattern so the pipeline can ingest data from any structured source -- RSS, Atom, EDI 834, and Zotero research libraries. Proves the architecture works across content feeds, transactional healthcare data, and hybrid local-file-plus-API sources.

- PRD: [.orchestra/work/multi-source-ingestion/prd.md](.orchestra/work/multi-source-ingestion/prd.md)
- Dependency: Foundation
- Status: Complete

### Data Transformation

Adds a composable transformation layer between ingestion and storage. Handles deduplication, content enrichment, validation, and a three-tier output pattern (raw / curated / rejected). Turns Conduit from a data fetcher into a data pipeline.

- PRD: [.orchestra/work/data-transformation/prd.md](.orchestra/work/data-transformation/prd.md)
- Dependency: Multi-Source Ingestion
- Status: Complete

### Source Depth: EDI 834

Deepens the 834 adapter from a working prototype to a more complete X12 implementation. Adds transaction/batch envelope tracking, functional acknowledgments (999/TA1), effective dating for overlapping coverage periods, and a more complete X12 loop parser for real-world 834 files.

- PRD: [.orchestra/work/source-depth-edi834/prd.md](.orchestra/work/source-depth-edi834/prd.md)
- Dependency: Data Transformation
- Status: Not Started

### Source Depth: Zotero

Deepens the Zotero adapter beyond CSV parsing and domain tagging. Adds richer metadata resolution (CrossRef API for citation counts and venue), preprint-to-published version linking, collection and tag hierarchy, and reading status tracking.

- PRD: [.orchestra/work/source-depth-zotero/prd.md](.orchestra/work/source-depth-zotero/prd.md)
- Dependency: Data Transformation
- Status: Not Started

### Source Depth: RSS

Deepens the RSS adapter beyond keyword extraction. Adds content-similarity deduplication across feeds, topic clustering, feed health tracking, and full-text extraction from linked articles.

- PRD: [.orchestra/work/source-depth-rss/prd.md](.orchestra/work/source-depth-rss/prd.md)
- Dependency: Data Transformation
- Status: Not Started

### Storage Backends

Introduces persistent storage options beyond the local filesystem — database backends (SQLite, PostgreSQL), cloud storage (S3, Azure Blob), or hybrid strategies. Builds on the storage abstraction established in the Data Transformation milestone so that switching or adding backends is a configuration change, not a code change.

- PRD: To be created
- Dependency: Data Transformation
- Status: Not Started

## References

- ADR-000: [The Score](adr/ADR-000-the-score.md)
- ADR-001: [Domain-Agnostic Pipeline](adr/ADR-001-domain-agnostic-pipeline.md)
- ADR-002: [Production Readiness is Continuous](adr/ADR-002-production-readiness-is-continuous.md)
- ADR-003: [Docusaurus Deploys .orchestra/ to GitHub Pages](adr/ADR-003-no-docs-site.md)
