# Spec: Data Transformation

**PRD:** [prd.md](prd.md)
**Status:** Complete

## Objective

Insert a composable transformation layer between ingestion and storage so that raw records are deduplicated, enriched, and written to a separate transformed output location — while preserving raw data unchanged.

## Approach

### Step 1: Define the transform stage interface

Create `ITransform` in `Conduit.Core` — a single method that takes a list of `IPipelineRecord` and returns a (potentially filtered/modified) list. Each stage is a discrete, independent unit of work (dedup, enrichment, validation, etc.).

This interface is the composability contract. Adding a new stage means implementing one interface and registering it — no changes to the pipeline or other stages.

### Step 2: Build the transform pipeline orchestrator

Create a service that accepts an ordered list of `ITransform` instances and runs records through them sequentially. The pipeline sits between `adapter.IngestAsync()` and `writer.WriteAsync()` in the existing flow:

```
adapter.IngestAsync() → transform pipeline → writer.WriteAsync()
```

The orchestrator replaces nothing — it's inserted into the existing loop in `Program.cs` (console), `Worker.cs`, and the API's POST endpoint. The raw writer continues to write to `data/raw/{sourceType}/` before transformation. The transform pipeline writes its output to `data/curated/{sourceType}/`.

### Step 3: Implement deduplication stage

Build a dedup stage that filters out records already seen in previous runs. This requires reading previous transformed output to build a set of known IDs.

Dedup key strategy per source type (from PRD):
- **RSS/Atom:** Link URL (the `Id` property on `FeedItem`)
- **Zotero:** DOI first, fall back to URL (the `Id` property on `ResearchRecord`)
- **EDI 834:** Subscriber ID + coverage start date + plan ID (composite key)

The dedup stage needs a way to resolve the dedup key per record type. The simplest approach: use `IPipelineRecord.Id` as the default key, with an optional override mechanism for composite keys (EDI 834).

### Step 4: Implement content enrichment stage

Build one enrichment stage per source type to prove the pattern:

- **RSS/Atom:** Extract keywords from title and description (simple term frequency — no external API calls needed for MVP)
- **Zotero:** Derive research domain tags from abstract content
- **EDI 834:** Derive enrollment status ("active" / "terminated") from maintenance type codes and coverage end dates

Enrichment adds new fields to the record. This means the transformed output schema is a superset of the raw schema — the original fields are preserved, new fields are added.

Enrichment wraps the original record in a `TransformedRecord<T>` envelope — the raw record is preserved untouched, and derived fields live in a separate enrichment dictionary. This approach works uniformly across all source types without modifying domain models.

This matters because `EnrollmentRecord` is an immutable positional `record` type — you can't add nullable properties to it without changing its constructor. The envelope sidesteps this entirely. It also makes the raw-vs-derived distinction visible in the output: the `record` field is what the source said, the `enrichment` field is what we concluded.

Serialized output looks like:
```json
{
    "record": { "subscriberId": "123", "maintenanceTypeCode": "024", ... },
    "enrichment": { "enrollmentStatus": "terminated" }
}
```

### Step 5: Extend storage to support reads

The current `IOutputWriter` only writes. Dedup needs to read previous transformed output to build a set of known IDs, and the API/CLI need to query stored data. Extend the storage interface to support:
- Write (persist transformed records — already exists)
- Read previous records by source type (for dedup lookups)
- Query/list (for API and CLI consumption)

`JsonOutputWriter` remains the sole backend for this milestone. It writes transformed output to `data/curated/{sourceType}/` and reads from the same location for dedup and queries.

### Step 6: Wire up DI and configuration

Register the transform stages in all four entry points (Console, Worker, API, CLI). Add configuration to `appsettings.json`:

- Which transform stages are enabled (dedup, enrichment)
- Transformed output directory path

### Step 7: Update API and CLI to read from transformed output

The API's `GET /sources/{name}/items` and the CLI's `list` and `search` commands currently read from `data/`. Update them to read from the transformed output location by default. The raw `data/` folder remains accessible but is no longer the primary consumer-facing output.

### Step 8: Tests

For each new component:
- **Transform pipeline orchestrator:** stages execute in order, empty input passes through, stages can filter records
- **Dedup stage:** duplicate records filtered, unique records pass through, idempotent on repeated runs, per-source-type key strategies work correctly
- **Enrichment stage:** envelope preserves original record, enrichment fields added, enrichment is deterministic (idempotent)
- **Storage reads:** JSON backend reads previous transformed output correctly for dedup lookups
- **Integration:** end-to-end pipeline with real fixtures produces expected transformed output

## Deliverables

| Deliverable | Location | Status |
|-------------|----------|--------|
| `ITransform` interface | `src/Core/Conduit.Core/Services/ITransform.cs` | Done |
| `TransformedRecord<T>` envelope type | `src/Core/Conduit.Core/Models/TransformedRecord.cs` | Done |
| Transform pipeline orchestrator | `src/Core/Conduit.Core/Services/TransformPipeline.cs` | Done |
| Deduplication transform (with cross-run dedup) | `src/Core/Conduit.Core/Services/DeduplicationTransform.cs` | Done |
| `ICompositeDedupKey` for composite keys | `src/Core/Conduit.Core/Models/ICompositeDedupKey.cs` | Done |
| Enrichment transforms (RSS, Zotero, EDI 834) | `src/Transforms/Conduit.Transforms/` | Done |
| `ITransformedOutputWriter` (write + read) | `src/Core/Conduit.Core/Services/ITransformedOutputWriter.cs` | Done |
| `JsonTransformedOutputWriter` | `src/App/Conduit/Services/JsonTransformedOutputWriter.cs` | Done |
| Config additions (`CuratedOutputDir`) | `src/App/Conduit/appsettings.json` | Done |
| DI registration updates | All four `Program.cs` files | Done |
| API/CLI updated to read curated output | `src/App/Conduit.Api/`, `src/App/Conduit.Cli/` | Done |
| Transform tests (46 tests) | `tests/Conduit.Transforms.Tests/` | Done |

## Acceptance Criteria

- [x] Processing the same RSS feed twice produces only one copy of each article in transformed output
- [x] Processing the same EDI 834 file twice produces only one copy of each enrollment record
- [x] Transformed RSS records include extracted keywords not present in the raw output
- [x] Transformed EDI 834 records include a derived enrollment status field
- [x] Removing the enrichment stage from config still produces deduplicated output (stages are independent)
- [x] Adding a new no-op transform stage requires only one new class and one DI registration
- [x] `data/raw/{sourceType}/` contains raw output; `data/curated/{sourceType}/` contains processed output
- [x] The pipeline is idempotent — running it N times on the same input produces the same transformed output as running it once
- [x] All existing tests continue to pass (raw pipeline is not broken)

## Dependencies

- Multi-Source Ingestion milestone (complete) — all four adapters exist and work
- `IPipelineRecord` interface — the contract for records flowing through the pipeline
- `IOutputWriter` — will be refactored, not replaced
- `appsettings.json` configuration pattern — already established

## Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| Enrichment envelope adds serialization complexity | Transformed JSON schema differs from raw, could break consumers | Envelope has a clear `record` + `enrichment` structure; consumers know which fields are original vs. derived |
| Dedup via JSON file scanning is slow at scale | Performance degrades as data accumulates | Acceptable for current data volumes; a database backend can be added in a future milestone if needed |
| Four entry points need identical DI registration | Registration drift between Console/Worker/API/CLI | Extract shared registration into a common extension method (e.g., `services.AddConduitPipeline()`) |
| Composite dedup keys (EDI 834) add complexity | Simple ID-based dedup doesn't work for all source types | Default to `IPipelineRecord.Id`, allow stages to override key resolution per source type |
