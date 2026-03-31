# PRD: Source Depth: Zotero

**Milestone:** Source Depth: Zotero
**Status:** Not Started

## Objective

Deepen the Zotero adapter beyond CSV parsing and keyword tagging — adding richer metadata, version linking, and reading status so the pipeline produces research records worth acting on.

## Success Criteria

- [ ] Citation counts and publication venue are resolved via the CrossRef API and surfaced in enriched output
- [ ] Preprint records are linked to their published versions where a DOI relationship exists
- [ ] Zotero collection hierarchy and tags are preserved in the pipeline record
- [ ] Reading status (unread, in-progress, read) from Zotero metadata is captured and surfaced
- [ ] All new behavior is covered by tests and documented

## Context

The current Zotero adapter reads a CSV export and applies domain tagging. This is enough to prove the adapter pattern works, but the resulting records are thin — a title, an identifier, a few tags. A research monitoring use case needs more: how often is this paper cited, where was it published, is this a preprint or the final version, and has anyone on the team read it?

This milestone fills those gaps by pulling from CrossRef (already used in the Zotero adapter for arXiv enrichment) and making better use of the metadata Zotero already tracks. The result is a research record rich enough to support prioritization and triage workflows.

## Materials

| Deliverable | Location | Status |
|-------------|----------|--------|
| PRD | `.orchestra/work/source-depth-zotero/prd.md` | Complete |
| Spec | `.orchestra/work/source-depth-zotero/spec.md` | Not Started |

## References

- Milestone: [Source Depth: Zotero](.orchestra/roadmap.md)
- ADR-001: [Domain-Agnostic Pipeline](../../adr/ADR-001-domain-agnostic-pipeline.md)
