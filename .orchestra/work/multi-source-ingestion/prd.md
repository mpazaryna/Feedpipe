# Multi-Source Ingestion

**Objective:** Enable the pipeline to ingest content from any structured source -- not just RSS -- so that a single deployment can aggregate data across formats and providers.

## Success Criteria

- [ ] Adding a new source type requires implementing one interface, not changing the pipeline core
- [ ] At least three source types work end-to-end (RSS, Atom, REST API)
- [ ] The pipeline auto-detects the format of a feed URL without user configuration
- [ ] Multiple sources are processed concurrently without blocking each other
- [ ] A failing source does not prevent other sources from completing

## Context

The current pipeline only understands RSS 2.0. In practice, content worth aggregating comes from many formats: Atom feeds, REST APIs, webhooks, file drops. A pipeline locked to one format forces users to find other tools or build custom integrations for every new source.

This milestone makes Feedpipe useful across domains. A news team needs RSS and Atom. A research team needs PubMed and arxiv APIs. A healthcare team needs FHIR endpoints. The value of the pipeline scales with the number of sources it can handle without custom code.

## Materials

| Material | Location | Status |
|----------|----------|--------|
| Source adapter interface | src/Feedpipe.Core/Services/ | Not Started |
| Atom feed adapter | src/Feedpipe/Services/ | Not Started |
| REST API adapter | src/Feedpipe/Services/ | Not Started |
| Format auto-detection | src/Feedpipe/Services/ | Not Started |
| Concurrent processing | src/Feedpipe/ | Not Started |
| Adapter tests | tests/Feedpipe.Tests/ | Not Started |

## Notes

This milestone PRD needs a spec before implementation. Run `/orchestra:spec` when ready.
