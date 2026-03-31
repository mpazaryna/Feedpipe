# ValidationTransform: Reject Invalid Records Before Curation

**Source:** https://app.clickup.com/t/86e0mhar6
**Priority:** —
**Date:** 2026-03-30

## Brief

Build a `ValidationTransform` that checks records against source-specific rules before they reach the curated tier. Invalid records are filtered out of the pipeline and routed to the rejected output (see sibling ticket). Validation must be domain-agnostic — one transform, pluggable rules per source type.

## Validation Rules by Source Type

### EDI 834 (EnrollmentRecord)
- **Required fields:** SubscriberId, MemberName, MaintenanceTypeCode, PlanId must be non-empty
- **Code set validation:** MaintenanceTypeCode must be a known value (021=addition, 024=termination, 001=change, 025=reinstatement). RelationshipCode must be valid (18, 01, 19, etc.)
- **Date logic:** CoverageEndDate, if present, must be after CoverageStartDate. CoverageStartDate must not be in the far future (>1 year)
- **Composite key integrity:** The dedup key fields (SubscriberId + CoverageStartDate + PlanId) must all be present

### RSS / Atom (FeedItem)
- **Required fields:** Title and Link must be non-empty
- **URL validation:** Link should be a valid absolute URL (not relative or malformed)
- **Date sanity:** PublishedDate should not be DateTime.MinValue (the fallback when feeds have no date — signals an incomplete record)

### Zotero (ResearchRecord)
- **Required fields:** Title must be non-empty, and at least one identifier (Doi or Url) must be present
- **DOI format validation:** If Doi is present, it should match the DOI pattern (10.xxxx/...)
- **Access level consistency:** If Abstract is empty and AccessLevel is Open, flag as suspect

## Acceptance Criteria

- [ ] Valid records pass through unchanged for all three source types
- [ ] Invalid records are removed from the pipeline output
- [ ] Each invalid record carries a list of validation errors (which rules failed)
- [ ] Validation is a composable ITransform — can be enabled/disabled independently
- [ ] Tests cover each validation rule with valid and invalid fixtures for EDI 834, RSS, and Zotero
- [ ] The validation framework is extensible — adding rules for a new source type does not require changes to existing rules

## Notes

- This is a domain-agnostic pipeline, not just an 834 pipeline. Every source type gets validation — the rules differ, the pattern is the same.
- Invalid records need to be captured (not just dropped) — depends on the rejected tier ticket (86e0mhara, complete).
- Follows TDD: tests first, then implementation.
