# 2026-03-30: Rejected Data Tier — Completing the Medallion Pattern

## What Happened

Implemented the `data/rejected/` output tier from ticket 86e0mhara, completing the three-tier medallion pattern: raw → curated + rejected.

The work covered the full orchestra loop in one session: ticket capture → PRD → spec → implementation → devlog. First time through the complete loop on this project.

## Why It Matters

Conduit previously had no place for records that fail validation. They would silently disappear from curated output with no audit trail. In regulated domains like healthcare (EDI 834), that's a compliance problem — you need to know which records were rejected and why.

The rejected tier closes that gap. Every record that fails validation lands in `data/rejected/{sourceType}/` with the original record preserved intact and human-readable error messages explaining the failure.

## Key Design Decisions

### `RejectedRecord<T>` mirrors `TransformedRecord<T>`

The envelope pattern was already established for curated output — wrap the original record untouched, add derived fields alongside it. The rejected tier follows the same shape: original record + errors + rejectedAt timestamp. Consistency makes the codebase easier to navigate and the output easier to reason about.

### `IRejectedOutputWriter` is a first-class interface in Core

Putting the interface in `Conduit.Core` (dependency-free) keeps the same pattern as `IOutputWriter` and `ITransformedOutputWriter`. The implementation lives in the app layer. This means the ValidationTransform (the next ticket) can depend on the interface without pulling in the filesystem implementation.

### Default parameter on `AddConduitPipeline()`

Adding `rejectedOutputDir = "data/rejected"` as a default parameter meant zero call-site changes were required — the existing code continued to compile. The three entry points (Console, Worker, API) were updated to pass the value explicitly from `AppSettings`, but they would have worked with the default too.

## What We Shipped

- `RejectedRecord<T>` model in `Conduit.Core`
- `IRejectedOutputWriter` interface in `Conduit.Core`
- `JsonRejectedOutputWriter` implementation
- `RejectedOutputDir` config in `AppSettings` and all three `appsettings.json` files
- DI registration in `AddConduitPipeline()`
- 5 unit tests in `JsonRejectedOutputWriterTests`
- Integration test that writes to the real `data/rejected/` directory

The integration test was added specifically so the output is visible on disk after running the test — useful for learning and verification without waiting for the ValidationTransform to wire rejected records into the live pipeline.

## What's Next

This ticket provides the output destination. The ValidationTransform ticket provides the records to write. Until that transform is built, `data/rejected/` only fills from the integration test. The two tickets together complete the validation story.

## Learning Note

Good example of the interface → implementation → DI pattern in .NET. The interface lives in `Conduit.Core` with no external dependencies. The implementation lives in the app layer and takes a logger via constructor injection. The DI container wires them together at startup. The calling code (pipeline, tests) only ever sees the interface — it doesn't know or care whether output goes to a file, a database, or nowhere.
