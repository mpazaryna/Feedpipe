# Feedpipe Roadmap

**Objective:** A production-ready .NET data pipeline that fetches, transforms, and serves content from multiple sources with pluggable adapters, transformation stages, and resilient delivery.

## Success Criteria

- [ ] Multi-source ingestion beyond RSS (Atom, REST APIs)
- [ ] Data transformation and enrichment layer
- [ ] Production-grade resilience (retries, health checks, monitoring)
- [ ] Deployable as a container to Azure or any Linux host
- [ ] Comprehensive test coverage and documentation

## Context

Feedpipe is a content aggregation and data pipeline platform built on .NET 10. It ingests content from diverse sources, transforms and enriches it, and serves it through multiple interfaces. The architecture follows a staged pipeline: fetch -> parse -> transform -> store, with multiple entry points (console runner, background worker, REST API, CLI).

The pipeline pattern is domain-agnostic -- applicable to news aggregation, research monitoring, competitive intelligence, healthcare data ingestion, or any scenario where structured content needs to be collected from heterogeneous sources.

## Milestones

| Material | Location | Status |
|----------|----------|--------|
| Foundation | .orchestra/work/foundation/prd.md | Done |
| Multi-Source Ingestion | .orchestra/work/multi-source-ingestion/prd.md | Not Started |
| Data Transformation | .orchestra/work/data-transformation/prd.md | Not Started |
| Production Hardening | .orchestra/work/production-hardening/prd.md | Not Started |

## References

- ADR-000: [The Score](.orchestra/adr/ADR-000-the-score.md)
