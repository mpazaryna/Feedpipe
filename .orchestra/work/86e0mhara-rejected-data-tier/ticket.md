# Rejected Data Tier: data/rejected/ for Invalid Records

**Source:** https://app.clickup.com/t/86e0mhara
**Priority:** —
**Date:** 2026-03-30

## Brief

Add a `data/rejected/` output tier alongside `data/raw/` and `data/curated/`. Records that fail validation land here with error details, preserving the three-tier enterprise data pattern: raw → curated (valid) + rejected (invalid).

## Deliverables

- `IRejectedOutputWriter` interface in Conduit.Core
- `JsonRejectedOutputWriter` implementation writing to `data/rejected/{sourceType}/`
- `RejectedOutputDir` config in AppSettings and appsettings.json
- Registration in `AddConduitPipeline()` extension method
- Pipeline wiring: ValidationTransform routes invalid records to rejected writer

## Output Format

Each rejected record should include the original record plus validation errors:

```json
{
  "record": { "subscriberId": "123", ... },
  "errors": [
    "MaintenanceTypeCode 999 is not a valid code",
    "CoverageEndDate is before CoverageStartDate"
  ],
  "rejectedAt": "2026-03-29T..."
}
```

## Acceptance Criteria

- [ ] `data/rejected/{sourceType}/` directory is created on first rejected record
- [ ] Rejected output includes the original record and a list of human-readable error messages
- [ ] Raw output is unaffected (invalid records still appear in raw)
- [ ] Curated output excludes rejected records
- [ ] The rejected writer follows the same JSON file naming pattern as raw and curated
- [ ] Registered via `AddConduitPipeline()` — no separate DI setup needed

## Notes

- Depends on ValidationTransform ticket — the transform produces the rejections, this ticket provides the output destination.
- The three-tier pattern (raw/curated/rejected) is standard in enterprise data pipelines and maps to the medallion architecture with an error sidecar.
- Follows TDD.
