# 2026-03-29: Data Transformation — From PRD to Working Code

## What Happened

Built the entire Data Transformation milestone in one session: PRD refinement, spec writing, TDD implementation across 8 steps, 43 new tests, and a working end-to-end pipeline that deduplicates and enriches records from all four source types.

## The PRD Journey

The existing PRD was a skeleton with technical implementation details in the wrong places. We rewrote it business-first:

- Added per-source-type expectations — what "duplicate" and "enrichment" actually mean for RSS, Zotero, and EDI 834
- Added three real-world use cases: open enrollment reconciliation (EDI 834), power user with 120+ RSS feeds, PhD student's Zotero library
- Established raw/transformed data separation as a first-class requirement
- Added idempotency as a success criterion (the Worker runs every 5 minutes)
- Scoped storage to files-only for this milestone, with a future Storage Backends milestone on the roadmap

The use cases were the most valuable addition. They ground the technical work in actual problems and make the milestone legible to anyone reading the PRD.

## Key Design Decisions

### The Envelope Pattern

The biggest decision was how enrichment adds fields to records. Three options:

1. **Add nullable properties to domain models** — simple but `EnrollmentRecord` is an immutable positional `record` type, can't add properties without changing its constructor
2. **Subclass each model** (`EnrichedFeedItem : FeedItem`) — creates a parallel class hierarchy
3. **Envelope wrapper** (`TransformedRecord<T>`) — wraps the original record untouched, enrichment lives in a separate dictionary

We went with the envelope. The EDI 834 case drove the decision: the raw data says `MaintenanceTypeCode: "024"`, and the enrichment says `enrollmentStatus: "terminated"`. These are fundamentally different things — what the source said vs. what we concluded — and the envelope makes that distinction visible in the output.

### Composite Dedup Keys

`IPipelineRecord.Id` works for RSS (link URL) and Zotero (DOI/URL), but EDI 834 uniqueness is subscriber ID + coverage start date + plan ID. Rather than hardcoding source-type checks in the dedup stage, we created `ICompositeDedupKey` — an opt-in interface that `EnrollmentRecord` implements. The dedup stage checks for it and falls back to `Id` when absent. Clean, extensible, no coupling between Core and adapter projects.

### Where Things Live

- `ITransformStage`, `TransformPipeline`, `DeduplicationStage`, `TransformedRecord<T>`, `ICompositeDedupKey` → `Conduit.Core` (no adapter dependencies)
- `RssEnrichmentStage`, `Edi834EnrichmentStage`, `ZoteroEnrichmentStage` → `Conduit.Transforms` (new project, references Core + adapters)
- `JsonTransformedOutputWriter` → `Conduit` console app (alongside `JsonOutputWriter`)

The enrichment stages needed adapter model references (to pattern-match on `FeedItem`, `EnrollmentRecord`, `ResearchRecord`), so they couldn't live in Core. A separate `Conduit.Transforms` project keeps the dependency graph clean.

## What We Built

| Component | Tests | Purpose |
|-----------|-------|---------|
| `TransformedRecord<T>` | 5 | Envelope wrapping raw record + enrichment dictionary |
| `TransformPipeline` | 6 | Chains stages sequentially between ingest and write |
| `DeduplicationStage` | 8 | Filters duplicates by ID or composite key |
| `RssEnrichmentStage` | 5 | Extracts keywords from title/description |
| `Edi834EnrichmentStage` | 5 | Derives enrollment status and relationship labels |
| `ZoteroEnrichmentStage` | 3 | Classifies research domain from abstract |
| `JsonTransformedOutputWriter` | 6 | Writes envelopes to `data/transformed/`, reads IDs for dedup |
| Integration tests | 5 | End-to-end pipeline with real fixtures |

**Total: 43 new tests, 104 across the solution.**

## The Pipeline Flow Now

```
adapter.IngestAsync()
  → List<IPipelineRecord>
  → rawWriter.WriteAsync()          → data/{sourceType}/
  → transformPipeline.ExecuteAsync()
    → DeduplicationStage            (filters duplicates)
    → RssEnrichmentStage            (adds keywords to FeedItems)
    → Edi834EnrichmentStage         (adds status/relationship to EnrollmentRecords)
    → ZoteroEnrichmentStage         (adds domain tags to ResearchRecords)
  → transformedWriter.WriteAsync()  → data/transformed/{sourceType}/
```

All four entry points (Console, Worker, API, CLI) are wired. The API reads from `data/transformed/` by default. The CLI searches across all transformed output and displays enrichment metadata.

## What's Left

The PRD has 7 success criteria. Current status:

- [x] Dedup: same content stored only once
- [x] Enrichment: at least one derived signal per source type
- [x] Composable: stages added/removed without changing others
- [x] Storage extensible: structured for future backends
- [x] Raw/transformed separation
- [x] Idempotent on repeated runs
- [ ] Cross-run dedup (reading previous transformed output before dedup)

The dedup stage currently deduplicates within a single batch. Cross-run dedup (reading `data/transformed/` to check against previously stored records) is supported by `JsonTransformedOutputWriter.ReadPreviousIdsAsync()` but not yet wired into the dedup stage. That's the one remaining gap.
