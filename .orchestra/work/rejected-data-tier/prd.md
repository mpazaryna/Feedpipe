# Rejected Data Tier

**Objective:** Add a rejected output tier so that records failing validation are preserved with their error details — completing the raw → curated + rejected medallion pattern and giving downstream consumers a clear audit trail of what was invalid and why.

## Success Criteria

- [ ] Invalid records are captured in a separate rejected output location, not silently dropped
- [ ] Each rejected record includes the original record and human-readable reasons for rejection
- [ ] Raw output is unaffected — invalid records still appear in raw; the rejected tier is additive
- [ ] Curated output excludes rejected records
- [ ] The rejected output location is configurable
- [ ] No additional DI setup required at call sites

## Context

Conduit currently has two output tiers: raw (original ingested records) and curated (deduplicated and enriched records). There is no designated place for records that fail validation — they either silently disappear from curated output or cause pipeline errors with no record of what went wrong.

In regulated domains like healthcare (EDI 834), an invalid enrollment record can't just be dropped. It needs to be preserved with an explanation of why it was rejected — both for compliance and for the operations team to fix and reprocess. More broadly, seeing exactly which records were rejected and why is essential for diagnosing source data quality issues.

This deliverable provides the output destination for invalid records. It depends on a ValidationTransform that produces the rejections — together they close the loop: valid records go to curated, invalid records go to rejected, raw always has everything.

Serves the **Data Transformation** milestone.

## Materials

| Material | Location | Status |
|----------|----------|--------|
| Rejected output writer interface | `src/Core/Conduit.Core/` | Not Started |
| Rejected output writer implementation | `src/App/Conduit/Services/` | Not Started |
| Configuration | `src/App/Conduit/Models/` + `appsettings.json` | Not Started |
| DI registration | `src/App/Conduit/Services/ServiceCollectionExtensions.cs` | Not Started |
| Tests | `tests/` | Not Started |

## References

- Milestone PRD: [data-transformation/prd.md](../data-transformation/prd.md)
- Ticket: https://app.clickup.com/t/86e0mhara

## Notes

- Depends on ValidationTransform ticket — this provides the destination; that transform produces the records.
- The three-tier pattern (raw / curated / rejected) maps to the medallion architecture with an error sidecar.
