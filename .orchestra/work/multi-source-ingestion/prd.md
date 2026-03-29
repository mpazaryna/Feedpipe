# Multi-Source Ingestion

**Objective:** Enable the pipeline to ingest data from any structured source -- not just RSS -- so that a single deployment can aggregate and process data across formats, providers, and domains.

## Success Criteria

- [x] Adding a new source type requires no changes to the pipeline core
- [x] At least three source types work end-to-end (RSS, Atom, EDI 834)
- [x] The pipeline auto-detects feed format without user configuration
- [x] EDI 834 files can be ingested from the local filesystem
- [x] Multiple sources are processed concurrently without blocking each other
- [x] A failing source does not prevent other sources from completing
- [x] Content sources and transaction sources coexist without contaminating each other

## Context

The current pipeline only understands RSS 2.0. In practice, data worth processing comes in many formats: Atom feeds, REST APIs, EDI transactions, flat files. A pipeline locked to one format forces users to find other tools or build custom integrations for every new source.

This milestone makes Conduit useful across domains. A news team needs RSS and Atom. A research team needs PubMed and arxiv APIs. A healthcare team needs EDI 834 enrollment files processed reliably with validation and audit trails. The value of the pipeline scales with the number of sources it can handle without custom code.

EDI 834 is the healthcare industry standard for exchanging member enrollment data between employers, payers, and government agencies. It carries enrollments, terminations, demographic changes, and coverage details. Adding it as a source type proves the pipeline architecture works beyond content aggregation -- it handles structured transactional data with strict validation requirements.

## Prework Completed

- Source adapter interface exists and is proven with RSS
- Configuration supports a source type field for adapter selection
- RSS adapter is isolated in its own module with passing tests
- Error handling already prevents one failing source from crashing the pipeline

## Deliverables

- [x] IPipelineRecord base type with Id, Timestamp, SourceType
- [x] Keyed DI adapter routing based on SourceSettings.Type
- [x] Concurrent source processing via Task.WhenAll + SemaphoreSlim
- [x] EDI 834 adapter with EnrollmentRecord model (Conduit.Sources.Edi834)
- [x] 834 test fixtures and 12 adapter tests
- [x] RSS and 834 output to data/rss/ and data/edi834/ respectively
- [x] Atom feed support with format auto-detection
