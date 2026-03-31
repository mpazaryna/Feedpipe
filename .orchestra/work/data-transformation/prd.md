# Data Transformation

**Objective:** Turn raw ingested content into clean, enriched, deduplicated data that is ready for consumption -- so downstream users never have to deal with duplicates, noise, or unstructured fields.

## Success Criteria

- [ ] The same content appearing in multiple sources is detected and stored only once
- [ ] Ingested records are enriched with at least one derived signal (e.g., extracted keywords, categories, or summaries)
- [ ] Transformation steps are composable -- new processing stages can be added or removed without changing existing ones
- [ ] The storage layer is structured so that new backends (e.g., database, cloud storage) can be added without changes to the pipeline or transformation logic
- [ ] Raw ingestion output is preserved unchanged; transformed records are written to a separate location
- [ ] Running the pipeline multiple times on the same input produces the same transformed output (idempotent)

## Context

Right now, Conduit fetches data from four source types (RSS, Atom, EDI 834, Zotero) and writes raw JSON files to `data/raw/{sourceType}/`. That's useful for proving the pipeline works, but the output is unprocessed. The same article can appear in multiple RSS feeds and get stored twice. There's no categorization beyond what the source provides. The only storage option is the local filesystem.

This is the difference between a data fetcher and a data pipeline. A fetcher downloads; a pipeline curates. Users consuming Conduit output -- whether through the API, CLI, or downstream systems -- should receive clean, deduplicated, enriched content without building their own post-processing.

Adding a transformation layer also unlocks future capabilities: content scoring, summarization, alerting on novel items, and cross-source analysis. None of those are possible when the pipeline just dumps raw data to disk.

### What transformation means per source type

**RSS / Atom (FeedItem):** Articles have a title, link, description, and published date. The same article often appears across multiple feeds. Deduplication should match on link URL. Enrichment opportunities: keyword extraction from title/description, topic categorization, language detection.

**Zotero / arxiv (ResearchRecord):** Papers have title, authors, DOI, abstract, tags, and access level. Duplicates can appear when the same paper is in multiple Zotero collections or when an arxiv preprint later gets a DOI. Deduplication should match on DOI first, then fall back to title similarity. Enrichment opportunities: research domain classification from abstract, citation context, related paper grouping.

**EDI 834 (EnrollmentRecord):** Enrollment transactions carry subscriber ID, member name, maintenance type (addition/termination), coverage dates, and plan ID. Duplicates occur when the same enrollment file is processed twice. Deduplication should match on subscriber ID + coverage start date + plan ID. Enrichment is different here — it's not about adding new content but about deriving status (e.g., "active" vs. "terminated" based on maintenance type codes and coverage end dates).

Each source type has a different definition of "duplicate" and a different enrichment profile. The transformation layer must accommodate this without source-specific hardcoding — the dedup and enrichment logic should be configurable per source type.

### Use cases

**EDI 834: Open Enrollment reconciliation**

Every November, a company with 5,000 employees runs open enrollment. Their benefits administrator sends daily EDI 834 files to the health plan during the enrollment window — 15 files over two weeks. Each file contains that day's batch of enrollment changes.

By the end, the raw data is a mess. The same subscriber might appear three times: enrolled in Plan A on Day 1, corrected to Plan B on Day 2, then terminated on Day 3 when they changed their mind. You have 800 raw records across 15 files, many of which supersede or contradict earlier ones.

The transformation layer collapses these to what actually matters: 480 unique member-plan relationships reflecting the *final* state. Dedup resolves subscriber + plan combinations to the most recent action. Enrichment derives `enrollmentStatus: "terminated"` from the raw `MaintenanceTypeCode: "024"` — so downstream consumers don't need to know X12 code tables.

Who needs this: the claims processing team needs to know who is actively covered *right now*, not who was enrolled on Day 1 before corrections. The compliance team needs the raw files preserved for audit but works from the transformed view. The finance team wants a headcount of active enrollees per plan — impossible to get accurately from raw files full of duplicates and superseded records. Without the transformation step, someone is doing this reconciliation manually in Excel, and in healthcare, a wrong enrollment status means a claim gets denied or a member loses coverage they should have.

**RSS/Atom: Power user with 100+ subscriptions**

A tech lead follows 120 RSS feeds — industry blogs, company engineering blogs, release note feeds, security advisories, and HN/Reddit aggregators. They run Conduit on a schedule to pull everything into one place.

The problem: the same story appears everywhere. A major CVE gets posted to the official advisory feed, picked up by three security blogs, linked on HN, and summarized on two aggregator feeds. Without dedup, the output is 7 copies of the same story scattered across source files. Multiply that across every notable story and the daily output is 60% noise.

The transformation layer deduplicates on link URL, collapsing those 7 copies to one. Enrichment extracts keywords from the title and description — tagging an article with `["kubernetes", "security", "CVE-2026-1234"]` so the user can filter or search by topic across all 120 feeds without opening each item. The transformed output is a single, deduplicated, categorized feed of everything that actually happened today — not a firehose of redundant entries.

**Zotero: PhD student doing background research**

A PhD candidate in computational biology is six months into their literature review. Their Zotero library has 400+ papers collected from Google Scholar, PubMed, arxiv, and conference proceedings. Papers get added in bursts — after a supervisor meeting, after reading a survey, after discovering a new sub-field.

The duplicates are subtle. The same paper appears as an arxiv preprint (added in January) and later as a published journal version (added in April) with a different URL but the same DOI. Another paper was saved from two different survey bibliographies with slightly different metadata. A third was added to three different Zotero collections for three different thesis chapters.

The transformation layer deduplicates on DOI first, then falls back to title similarity for papers without DOIs (common for conference papers and working drafts). Enrichment derives research domain tags from the abstract — classifying papers as `["machine learning", "protein folding", "drug discovery"]` so the student can see the landscape of their reading across sub-fields. When they sit down to write their related work section, they have a clean, categorized inventory of what they've read, not a sprawling Zotero library where the same paper appears under three names in four collections.

### Raw and transformed data are separate

Today the pipeline writes raw ingestion output to `data/raw/{sourceType}/`. Transformed records should be written to a distinct location (e.g., `data/curated/{sourceType}/`), not overwrite the raw output. This preserves the original data for debugging, reprocessing, and audit while making it clear which records have been through the pipeline's transformation stages.

The raw `data/raw/` folder is the "before" — exactly what the source provided. The transformed output is the "after" — deduplicated, enriched, and ready for consumption. Downstream consumers (API, CLI, other systems) should read from the transformed output by default.

### Storage: files now, extensible later

This milestone works entirely with static JSON files on the local filesystem — both raw output (`data/raw/{sourceType}/`) and transformed output (`data/curated/{sourceType}/`). There is no database, no cloud storage, no external dependencies.

However, the storage layer should be structured so that adding a new backend (SQLite, PostgreSQL, S3, etc.) is a future configuration change, not a rewrite. Deduplication already requires reading previous output, which means storage is a query surface, not just a dump target. Designing for that now avoids painting ourselves into a corner.

A dedicated **Storage Backends** milestone could be considered in the future to introduce database persistence, cloud storage, or hybrid strategies. That work would build on the abstraction established here.

## Deliverables

| Deliverable | Description | Status |
|-------------|-------------|--------|
| Transform pipeline | A way to chain processing stages between ingestion and storage | Not Started |
| Deduplication | Detect and filter duplicate records across sources and runs | Not Started |
| Content enrichment | Extract or derive at least one new field from raw content | Not Started |
| Storage abstraction | A common interface for persisting processed records | Not Started |
| Second storage backend | At least one alternative to filesystem JSON (e.g., SQLite) | Not Started |
| Tests | Coverage for each transformation stage and storage backend | Not Started |
| Rejected data tier | `data/rejected/` output for invalid records with error details | Complete — [86e0mhara-rejected-data-tier](../86e0mhara-rejected-data-tier/prd.md) |
| ValidationTransform | Filter invalid records before curation, route to rejected tier | Not Started — [86e0mhar6-validation-transform](../86e0mhar6-validation-transform/prd.md) |

## Notes

- This PRD needs a spec before implementation — run `/orchestra:spec data-transformation`
- Depends on Multi-Source Ingestion (complete)
- The existing `JsonOutputWriter` should become one implementation of the storage abstraction, not be replaced
- Enrichment scope is intentionally minimal for this milestone — one derived signal proves the pattern; more can be added later
