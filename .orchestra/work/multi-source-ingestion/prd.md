# Multi-Source Ingestion

**Objective:** Enable the pipeline to ingest data from any structured source -- not just RSS -- so that a single deployment can aggregate and process data across formats, providers, and domains.

## Success Criteria

- [x] Adding a new source type requires implementing one interface, not changing the pipeline core
- [ ] At least three source types work end-to-end (RSS, Atom, EDI 834)
- [ ] The pipeline auto-detects the format of feed-based sources (RSS vs Atom) without user configuration
- [ ] EDI 834 files can be ingested from the local filesystem with a clear path to SFTP/API ingestion
- [ ] Multiple sources are processed concurrently without blocking each other
- [x] A failing source does not prevent other sources from completing
- [ ] Content-based sources (RSS, Atom) and record-based sources (834) coexist in the same pipeline without contaminating each other's domain types

## Context

The current pipeline only understands RSS 2.0. In practice, data worth processing comes in many formats: Atom feeds, REST APIs, EDI transactions, flat files. A pipeline locked to one format forces users to find other tools or build custom integrations for every new source.

This milestone makes Conduit useful across domains. A news team needs RSS and Atom. A research team needs PubMed and arxiv APIs. A healthcare team needs EDI 834 enrollment files processed reliably with validation and audit trails. The value of the pipeline scales with the number of sources it can handle without custom code.

EDI 834 (ANSI X12 Benefit Enrollment and Maintenance) is the healthcare industry standard for exchanging member enrollment data between employers, payers, and government agencies. It carries enrollments, terminations, demographic changes, and coverage details. Adding it as a source type proves the pipeline architecture works beyond content aggregation -- it handles structured transactional data with strict validation requirements.

## Materials

| Material | Location | Status |
|----------|----------|--------|
| ISourceAdapter interface | src/Conduit.Core/Services/ISourceAdapter.cs | Done |
| IOutputWriter interface | src/Conduit.Core/Services/IOutputWriter.cs | Done |
| SourceSettings config with Type field | src/Conduit/Models/FeedSettings.cs | Done |
| RSS adapter (isolated project) | src/Conduit.Sources.Rss/ | Done |
| RSS adapter tests | tests/Conduit.Sources.Rss.Tests/ | Done |
| Atom feed adapter | src/Conduit.Sources.Rss/ | Not Started |
| Feed format auto-detection (RSS vs Atom) | src/Conduit.Sources.Rss/ | Not Started |
| Adapter registration by Type in DI | src/Conduit/ | Not Started |
| Concurrent source processing | src/Conduit/ | Not Started |
| EDI 834 adapter | src/Conduit.Sources.Edi834/ | Not Started |
| EnrollmentRecord model | src/Conduit.Sources.Edi834/ | Not Started |
| 834 sample test fixtures | tests/Conduit.Sources.Edi834.Tests/fixtures/ | Not Started |
| 834 adapter tests | tests/Conduit.Sources.Edi834.Tests/ | Not Started |

## Notes

Several items are already done from the foundation refactor:
- `ISourceAdapter` and `IOutputWriter` interfaces exist in Core
- `SourceSettings` has a `Type` field for adapter selection
- RSS adapter is isolated in its own project with 6 passing tests
- Error handling already prevents one failing source from crashing the pipeline

Remaining work falls into three tracks:
1. **Atom support** -- add to the existing RSS source project since Atom is a content feed format, sharing the same FeedItem model. Include auto-detection so users don't need to specify RSS vs Atom.
2. **Adapter routing** -- the pipeline currently hardcodes `RssSourceAdapter` in DI. It needs to resolve the correct adapter based on `SourceSettings.Type` at runtime.
3. **EDI 834** -- new source project with its own domain model (`EnrollmentRecord`), parser, and test fixtures. This is the proof that the architecture handles fundamentally different data shapes.

834 parsing involves X12 segment structure: ISA envelope, GS functional group, ST transaction set, and member loops (2000) containing INS, REF, DTP, NM1, and other segments.
