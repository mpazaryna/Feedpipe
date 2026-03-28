# Data Transformation

**Objective:** Add a transformation layer between ingestion and storage that can enrich, deduplicate, and normalize content before it's persisted.

## Success Criteria

- [ ] Transformation pipeline with composable stages
- [ ] Content deduplication (by URL or content hash)
- [ ] At least one enrichment step (e.g., keyword extraction, categorization)
- [ ] Alternative storage backend beyond JSON files (e.g., SQLite, Azure Blob)
- [ ] Storage adapter interface in Feedpipe.Core
- [ ] Tests for transformation and dedup logic

## Context

Part of the [Feedpipe Roadmap](../../roadmap.md). The current pipeline goes straight from fetch to file write. Production pipelines need a transformation layer to clean, enrich, and normalize data before storage. This milestone introduces middleware-style processing -- a pattern used heavily in ASP.NET and data pipeline architectures.

## Materials

| Material | Location | Status |
|----------|----------|--------|
| ITransformStage interface | src/Feedpipe.Core/Services/ | Not Started |
| Deduplication transform | src/Feedpipe/Services/ | Not Started |
| Content enrichment transform | src/Feedpipe/Services/ | Not Started |
| IStorageBackend interface | src/Feedpipe.Core/Services/ | Not Started |
| SQLite storage backend | src/Feedpipe/Services/ | Not Started |
| Transform pipeline orchestrator | src/Feedpipe/Services/ | Not Started |
| Transform + storage tests | tests/Feedpipe.Tests/ | Not Started |

## Notes

This milestone PRD needs to be fleshed out. Run `/orchestra:prd` to expand it when ready.
