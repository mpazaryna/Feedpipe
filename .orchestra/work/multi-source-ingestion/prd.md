# Multi-Source Ingestion

**Objective:** Extend the pipeline to fetch from multiple source types beyond RSS 2.0, with a pluggable adapter pattern that makes adding new sources straightforward.

## Success Criteria

- [ ] Atom feed support alongside existing RSS
- [ ] At least one non-feed source (REST API, e.g. Hacker News API)
- [ ] Source adapter interface in Feedpipe.Core
- [ ] Auto-detection of feed format (RSS vs Atom)
- [ ] Multiple feeds processed concurrently
- [ ] Tests for each source adapter
- [ ] Existing tests still pass

## Context

Part of the [Feedpipe Roadmap](../../roadmap.md). The current pipeline only handles RSS 2.0 feeds. Real-world data pipelines ingest from diverse sources. This milestone introduces the adapter pattern -- a common .NET approach where each source type implements a shared interface, and the pipeline doesn't need to know which type it's talking to.

This is directly relevant to the healthcare pipeline role, where data will come from multiple systems (HL7, FHIR APIs, flat files).

## Materials

| Material | Location | Status |
|----------|----------|--------|
| ISourceAdapter interface | src/Feedpipe.Core/Services/ | Not Started |
| Atom feed adapter | src/Feedpipe/Services/ | Not Started |
| REST API adapter | src/Feedpipe/Services/ | Not Started |
| Feed format auto-detection | src/Feedpipe/Services/ | Not Started |
| Concurrent feed processing | src/Feedpipe/ | Not Started |
| Adapter tests | tests/Feedpipe.Tests/ | Not Started |

## Notes

This milestone PRD needs to be fleshed out. Run `/orchestra:prd` to expand it when ready.
