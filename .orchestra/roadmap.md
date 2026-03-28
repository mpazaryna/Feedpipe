# Feedpipe Roadmap

**Objective:** A production-ready .NET data pipeline that fetches, transforms, and serves content from multiple sources -- built as a learning vehicle and portfolio piece for onboarding to a healthcare data pipeline team.

## Success Criteria

- [ ] Multi-source ingestion beyond RSS (Atom, REST APIs)
- [ ] Data transformation and enrichment layer
- [ ] Production-grade resilience (retries, health checks, monitoring)
- [ ] Deployable as a container to Azure or any Linux host
- [ ] Comprehensive test coverage and documentation

## Context

Feedpipe started as a hands-on .NET learning project during onboarding prep for a healthcare data pipeline role starting 2026-03-30. The project demonstrates .NET patterns (DI, interfaces, Options, hosted services, minimal APIs) through a real working pipeline. The audience is both the developer (learning) and the team being joined (demonstrating competency).

The architecture follows a staged pipeline: fetch -> parse -> transform -> store, with multiple entry points (console, worker, API, CLI).

## Milestones

| Material | Location | Status |
|----------|----------|--------|
| Foundation | .orchestra/work/foundation/prd.md | Done |
| Multi-Source Ingestion | .orchestra/work/multi-source-ingestion/prd.md | Not Started |
| Data Transformation | .orchestra/work/data-transformation/prd.md | Not Started |
| Production Hardening | .orchestra/work/production-hardening/prd.md | Not Started |

## References

- ADR-000: [The Score](.orchestra/adr/ADR-000-the-score.md)
