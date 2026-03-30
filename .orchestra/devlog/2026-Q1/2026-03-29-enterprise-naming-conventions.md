# 2026-03-29: Why We Named Things the Way We Did

## Overview

Conduit is a learning project for building an enterprise-grade EDI 834 pipeline. The naming conventions matter — not because the code cares, but because the people who read and maintain it do. When you walk into a new team working on a healthcare data pipeline, the vocabulary is already established. Using the right terms means less translation overhead and fewer "what does this mean?" conversations.

This devlog captures the naming decisions and the industry context behind them.

## Data Tiers: `raw` and `curated`

Enterprise data pipelines universally organize output into tiers based on how processed the data is. The most common architecture is the "medallion" pattern (popularized by Databricks, adopted by AWS, Azure, and most data platform teams):

| Tier | Also Called | What It Means |
|------|------------|---------------|
| **Raw / Bronze** | Landing, Staging | Data exactly as it arrived from the source. No dedup, no enrichment, no transformation. Preserved for audit, reprocessing, and debugging. |
| **Curated / Silver** | Cleaned, Conformed | Deduplicated, validated, enriched. This is what consumers read. |
| **Aggregated / Gold** | Refined, Serving | Summarized, scored, or pre-computed for specific use cases (dashboards, reports, APIs). |

We use `data/raw/` and `data/curated/` — the first two tiers. We don't have a gold tier yet because there's no aggregation or scoring layer, but the naming leaves room for it.

**Why not `landing` and `staging`?** Those terms are more common in ETL batch systems (Informatica, SSIS) where "staging" implies a temporary holding area before loading into a warehouse. Our raw data isn't temporary — it's a permanent record. "Raw" and "curated" better describe the relationship: the raw data is the source of truth, the curated data is the refined view.

**Why not `bronze` and `silver`?** Those are Databricks-specific. `raw` and `curated` are understood across all platforms.

## Pipeline Operations: `Transforms` and `ITransform`

In data engineering, a **transform** is a unit of work that takes data in, modifies or filters it, and passes it out. This is the vocabulary used by:

- **Apache Beam** — `PTransform` (the core abstraction)
- **dbt** — models are transforms applied to source data
- **AWS Glue** — jobs contain transforms
- **Spark** — DataFrame transformations
- **Azure Data Factory** — data flow transformations

We use `ITransform` as the interface and `Conduit.Transforms` as the project name. Each implementation is a specific transform: `DeduplicationTransform`, `RssEnrichmentTransform`, `Edi834EnrichmentTransform`.

**Why not `Stage`?** "Stage" is more generic — it could mean anything in a pipeline (validation, routing, logging). "Transform" specifically means "I take data in and produce modified data out." Every transform is a stage, but not every stage is a transform. Since all our pipeline operations actually transform data (filter duplicates, add enrichment fields), `ITransform` is more precise.

**Why not `Processor`?** Common in Java/Spring ecosystems (e.g., Spring Batch `ItemProcessor`), but in the .NET and data engineering world, "transform" is the standard term.

## Project Structure: `Ingestion`, `Transforms`, `Core`

The project folders map to the standard data pipeline architecture:

```
src/
  Core/           → Domain contracts (interfaces, models)
  Adapters/       → Source connectors (RSS, EDI 834, Zotero)
  Transforms/     → Data processing (dedup, enrichment)
  App/            → Entry points (console, worker, API, CLI)
```

This mirrors how enterprise teams organize pipeline code:

- **Ingestion** (our Adapters) — the connectors that pull data from external systems. In healthcare, these would be SFTP listeners, AS2 receivers, or API pollers receiving 834/835/837 transactions.
- **Transforms** — the business logic that processes raw data into something useful. In an 834 pipeline, this includes deduplication across enrollment files, deriving member status from maintenance codes, and validating segment-level rules.
- **Persistence** (future) — the storage layer. In production, this would be a database (SQL Server is common in healthcare), not JSON files.

## EDI 834-Specific Terminology

For the 834 enrichment transform, we use terms from the X12 standard and healthcare industry:

| Our Term | X12 Origin | Business Meaning |
|----------|-----------|------------------|
| `enrollmentStatus` | Derived from INS03 (maintenance type) + DTP*349 (coverage end) | Whether a member is currently covered |
| `relationship` | INS02 (individual relationship code) | How the member relates to the subscriber (self, spouse, child) |
| `MaintenanceTypeCode` | INS03 | What action this transaction represents (021=addition, 024=termination) |
| `SubscriberId` | REF*0F | The primary subscriber identifier |
| `CoverageStartDate` / `CoverageEndDate` | DTP*348 / DTP*349 | The coverage period |
| `PlanId` | HD segment | The health plan identifier |
| `DedupKey` | Composite: subscriber + start date + plan | Uniqueness for enrollment dedup |

These aren't arbitrary names — they're what the business calls them. A benefits analyst or enrollment specialist will recognize "enrollment status" and "maintenance type code" without needing a glossary.

## The Envelope Pattern Naming

Transformed output uses a `TransformedRecord<T>` envelope with two fields:

```json
{
  "record": { ... },       // what the source said
  "enrichment": { ... }    // what we derived
}
```

In enterprise pipelines, this separation is called **lineage** or **provenance** — tracking what came from the source vs. what was computed. The envelope makes this visible in the data itself. When a claims processor asks "why does the system think this member is terminated?", you can show both the raw maintenance code (`024`) and the derived status (`terminated`) side by side.

## What This Means for the 834 Assignment

When you start the enterprise 834 pipeline assignment, the vocabulary transfers directly:

- **Raw** → where incoming 834 files land after receipt (via SFTP, AS2, or API)
- **Curated** → where deduplicated, validated, status-derived enrollment records live
- **Transforms** → the processing steps between raw and curated (dedup, validation, status derivation, eligibility calculation)
- **ITransform** → the contract each processing step implements

The code patterns transfer too — `DeduplicationTransform` with composite keys, `Edi834EnrichmentTransform` deriving status from maintenance codes, `TransformedRecord<T>` preserving lineage. These are the same problems at any scale.
