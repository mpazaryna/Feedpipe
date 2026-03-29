# Multi-Source Ingestion

**Objective:** Enable the pipeline to ingest data from any structured source -- not just RSS -- so that a single deployment can aggregate and process data across formats, providers, and domains.

## Success Criteria

- [ ] Adding a new source type requires implementing one interface, not changing the pipeline core
- [ ] At least three source types work end-to-end (RSS, Atom, EDI 834)
- [ ] The pipeline auto-detects the format of feed-based sources (RSS vs Atom) without user configuration
- [ ] EDI 834 files can be ingested from the local filesystem with a clear path to SFTP/API ingestion
- [ ] Multiple sources are processed concurrently without blocking each other
- [ ] A failing source does not prevent other sources from completing
- [ ] Content-based sources (RSS, Atom) and record-based sources (834) coexist in the same pipeline without contaminating each other's domain types

## Context

The current pipeline only understands RSS 2.0. In practice, data worth processing comes in many formats: Atom feeds, REST APIs, EDI transactions, flat files. A pipeline locked to one format forces users to find other tools or build custom integrations for every new source.

This milestone makes Conduit useful across domains. A news team needs RSS and Atom. A research team needs PubMed and arxiv APIs. A healthcare team needs EDI 834 enrollment files processed reliably with validation and audit trails. The value of the pipeline scales with the number of sources it can handle without custom code.

EDI 834 (ANSI X12 Benefit Enrollment and Maintenance) is the healthcare industry standard for exchanging member enrollment data between employers, payers, and government agencies. It carries enrollments, terminations, demographic changes, and coverage details. Adding it as a source type proves the pipeline architecture works beyond content aggregation -- it handles structured transactional data with strict validation requirements.

## Materials

| Material | Location | Status |
|----------|----------|--------|
| ISourceAdapter interface | src/Conduit.Core/Services/ | Not Started |
| IPipelineRecord base type | src/Conduit.Core/Models/ | Not Started |
| RSS adapter (refactor from RssFeedFetcher) | src/Conduit/Services/ | Not Started |
| Atom feed adapter | src/Conduit/Services/ | Not Started |
| EDI 834 adapter | src/Conduit/Services/ | Not Started |
| EnrollmentRecord model | src/Conduit.Core/Models/ | Not Started |
| Feed format auto-detection | src/Conduit/Services/ | Not Started |
| Concurrent source processing | src/Conduit/ | Not Started |
| Adapter tests (RSS, Atom, 834) | tests/Conduit.Tests/ | Not Started |

## Notes

The key design decision is the abstraction boundary between content sources (RSS/Atom) and record sources (834). Both flow through the same pipeline infrastructure, but their domain types must remain separate. FeedItem and EnrollmentRecord should share a common interface (IPipelineRecord) without sharing fields.

834 parsing involves X12 segment structure: ISA envelope, GS functional group, ST transaction set, and member loops (2000) containing INS, REF, DTP, NM1, and other segments. Sample 834 test files will be needed in the test fixtures.

This milestone PRD needs a spec before implementation. Run `/orchestra:spec` when ready.
