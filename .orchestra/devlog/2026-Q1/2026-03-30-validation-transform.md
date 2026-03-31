# 2026-03-30: ValidationTransform — Closing the Medallion Loop

## What Happened

Implemented the `ValidationTransform` from ticket 86e0mhar6, completing the raw → curated + rejected pipeline. Invalid records are now caught before curation, stored in `data/rejected/` with human-readable error details, and only clean records flow forward to deduplication and enrichment.

## Key Design Decisions

### `PipelineFactory` instead of extending `TransformPipeline.CreateForSource`

The first instinct was to add the new factory method directly to `TransformPipeline` in `Conduit.Core`. That failed at compile time — `TransformPipeline` is in Core, but `ValidationTransform` is in `Conduit.Transforms`, and Core cannot reference Transforms without creating a circular dependency.

The fix: `PipelineFactory` is a static class in `Conduit.Transforms`. It has full access to both Core infrastructure and Transform implementations. This is a good example of why dependency direction matters — the right place for something is determined by what it needs to reference, not just where it feels logical.

### `IRecordValidator` in Core, implementations in Transforms

The interface (`AppliesTo` + `Validate`) lives in `Conduit.Core` so the `ValidationTransform` can depend on it. The concrete validators (`EnrollmentRecordValidator`, `FeedItemValidator`, `ResearchRecordValidator`) live in `Conduit.Transforms` where the adapter models (`EnrollmentRecord`, `FeedItem`, `ResearchRecord`) are already available. Same pattern as the enrichment transforms.

### Validation runs before deduplication

Stage order matters: Validation → Dedup → Enrichment. An invalid record shouldn't consume a dedup slot or waste enrichment cycles. Catching it first also makes the rejected output cleaner — the record appears exactly once in `data/rejected/`, not partially processed.

### `Uri.TryCreate` surprise on .NET 10

A failing test exposed a .NET 10 behavior: `Uri.TryCreate("/path/to/article", UriKind.Absolute, out _)` returns `true` — it silently parses relative paths as `file://` URIs. The fix was checking that the URI scheme is `http` or `https`, not just that parsing succeeded. Good reminder that "absolute URI" and "web URL" are not the same thing.

## What We Shipped

- `IRecordValidator` interface in `Conduit.Core`
- `ValidationTransform` — splits valid/invalid, writes rejected records via `IRejectedOutputWriter`
- `EnrollmentRecordValidator` — 7 rules: required fields, X12 code sets, date logic
- `FeedItemValidator` — 3 rules: required fields, http/https URL, date sanity
- `ResearchRecordValidator` — 4 rules: title, identifier presence, DOI format, open-access abstract check
- `PipelineFactory` — replaces `TransformPipeline.CreateForSource` at all three call sites
- All three entry points wired (Console, Worker, API)
- 39 new tests (151 total); integration test writing to real `data/` directories

## What's Next

The three-tier pipeline is complete. The next natural step would be the Storage Backends milestone — introducing persistence options beyond the local filesystem (SQLite, PostgreSQL, S3).
