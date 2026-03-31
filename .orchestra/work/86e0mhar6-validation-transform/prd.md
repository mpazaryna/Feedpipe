# ValidationTransform

**Objective:** Add a validation stage to the pipeline so that records failing source-specific rules are caught before reaching curated output — invalid records are routed to the rejected tier with human-readable error details, and only clean records proceed to curation.

## Success Criteria

- [ ] Valid records pass through the validation stage unchanged for all three source types (EDI 834, RSS, Zotero)
- [ ] Invalid records are removed from curated output and written to `data/rejected/` with the errors that caused rejection
- [ ] Validation rules are source-specific but the validation stage itself is domain-agnostic — one composable `ITransform`, pluggable rules per source type
- [ ] Adding validation rules for a new source type requires no changes to existing rules or to the pipeline
- [ ] TDD: all rules have tests covering both valid and invalid cases

## Context

Conduit currently has no validation step. Every record that arrives from a source adapter goes straight to deduplication and enrichment, then to curated output — regardless of whether the data is actually valid. A malformed EDI 834 enrollment record with a missing subscriber ID or an impossible date range goes into `data/curated/` the same as a clean one.

The rejected tier (completed in 86e0mhara) provides the output destination for invalid records. This ticket builds the stage that produces them: a transform that checks each record against the rules for its source type, collects any failures as human-readable errors, and routes invalid records to the rejected writer instead of letting them flow to curated.

Validation rules differ per source type. EDI 834 needs code set validation and date logic. RSS needs URL and required field checks. Zotero needs identifier presence and DOI format checks. But the validation *pattern* is the same — pluggable rule sets, consistent error format, one composable stage in the pipeline.

Serves the **Data Transformation** milestone.

## Materials

| Material | Location | Status |
|----------|----------|--------|
| Validation rule interface | `src/Core/Conduit.Core/Services/` | Not Started |
| ValidationTransform | `src/Transforms/Conduit.Transforms/` | Not Started |
| EDI 834 validation rules | `src/Transforms/Conduit.Transforms/` | Not Started |
| RSS validation rules | `src/Transforms/Conduit.Transforms/` | Not Started |
| Zotero validation rules | `src/Transforms/Conduit.Transforms/` | Not Started |
| Pipeline wiring | `src/App/Conduit/Services/ServiceCollectionExtensions.cs` | Not Started |
| Tests | `tests/Conduit.Transforms.Tests/` | Not Started |

## References

- Milestone PRD: [data-transformation/prd.md](../data-transformation/prd.md)
- Depends on: [86e0mhara-rejected-data-tier](../86e0mhara-rejected-data-tier/prd.md) (complete)
- Ticket: https://app.clickup.com/t/86e0mhar6

## Notes

- The rejected tier is already built and registered in DI — this transform calls `IRejectedOutputWriter` to route invalid records there.
- Validation runs before enrichment — no point enriching a record that will be rejected.
- Rules documented in the ticket: EDI 834 (required fields, code sets, date logic, composite key integrity), RSS (required fields, URL format, date sanity), Zotero (required fields, DOI format, access level consistency).
