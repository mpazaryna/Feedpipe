# PRD: Source Depth: RSS

**Milestone:** Source Depth: RSS
**Status:** Not Started

## Objective

Deepen the RSS adapter beyond keyword extraction — adding cross-feed deduplication, topic clustering, feed health tracking, and full-text extraction so the pipeline delivers signal rather than noise.

## Success Criteria

- [ ] Articles appearing across multiple feeds are deduplicated by content similarity, not just URL equality
- [ ] Related articles are grouped into topic clusters in enriched output
- [ ] Feed health (last successful fetch, error rate, item frequency) is tracked and surfaced
- [ ] Full article text is extracted from linked URLs for feeds that publish summaries only
- [ ] All new behavior is covered by tests and documented

## Context

The current RSS adapter fetches and parses feeds, extracts keywords, and routes items through the standard dedup/enrich pipeline. For a single feed this works well. But news aggregation across many feeds surfaces a recurring problem: the same story appears ten times under different titles and URLs, topic-related articles are scattered, and a feed that has been silently broken for a week is indistinguishable from one that just has no news.

This milestone addresses those real-world aggregation challenges. Content-similarity deduplication catches the cross-feed duplicates that URL matching misses. Topic clustering turns a flat list of articles into a structured briefing. Feed health tracking surfaces operational issues before they affect downstream consumers. Full-text extraction makes keyword and summary quality independent of what publishers choose to include in their feed.

## Materials

| Deliverable | Location | Status |
|-------------|----------|--------|
| PRD | `.orchestra/work/source-depth-rss/prd.md` | Complete |
| Spec | `.orchestra/work/source-depth-rss/spec.md` | Not Started |

## References

- Milestone: [Source Depth: RSS](.orchestra/roadmap.md)
- ADR-001: [Domain-Agnostic Pipeline](../../adr/ADR-001-domain-agnostic-pipeline.md)
