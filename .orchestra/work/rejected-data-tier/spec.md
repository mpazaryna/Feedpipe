# Spec: Rejected Data Tier

**PRD:** [prd.md](./prd.md)
**Status:** Complete
**Ticket:** https://app.clickup.com/t/86e0mhara

## Objective

Add `data/rejected/` as a third output tier. Invalid records land here with their error details. Raw is unaffected. Curated excludes them.

## Approach

Follow TDD throughout — write a failing test, make it pass, move to the next step.

### Step 1: `RejectedRecord<T>` model

Add `src/Core/Conduit.Core/Models/RejectedRecord.cs`.

Mirrors `TransformedRecord<T>` in shape: wraps the original record untouched. Adds:
- `Errors: IReadOnlyList<string>` — human-readable validation failure messages
- `RejectedAt: DateTime` — when the record was rejected

### Step 2: `IRejectedOutputWriter` interface

Add `src/Core/Conduit.Core/Services/IRejectedOutputWriter.cs`.

Single method mirroring `IOutputWriter.WriteAsync` but accepting `List<RejectedRecord<IPipelineRecord>>` instead of raw records.

### Step 3: `JsonRejectedOutputWriter` implementation

Add `src/App/Conduit/Services/JsonRejectedOutputWriter.cs`.

Mirrors `JsonTransformedOutputWriter`:
- Writes to `{outputDir}/{sourceType}/{sourceName}_{timestamp}.json`
- Each entry serializes as `{ "record": {...}, "errors": [...], "rejectedAt": "..." }`
- Uses camelCase JSON, indented output
- Creates the source type subdirectory on first write

### Step 4: Configuration

Add `RejectedOutputDir` property to `AppSettings` in `src/App/Conduit/Models/FeedSettings.cs`. Default: `"data/rejected"`.

Add the corresponding key to `appsettings.json` under the `App` section.

### Step 5: DI registration

In `src/App/Conduit/Services/ServiceCollectionExtensions.cs`, add `IRejectedOutputWriter` registration to `AddConduitPipeline()`, mirroring the `IOutputWriter` and `ITransformedOutputWriter` registrations. Accept `rejectedOutputDir` as a third parameter.

Update all four entry point `Program.cs` files (Console, Worker, API, CLI) to pass the new parameter when calling `AddConduitPipeline()`, reading from `AppSettings.RejectedOutputDir`.

### Step 6: Tests

Add tests to `tests/Conduit.Tests/JsonRejectedOutputWriterTests.cs`, mirroring the structure of `JsonOutputWriterTests.cs` and `JsonTransformedOutputWriterTests.cs`.

Minimum test cases:
- Writing zero records creates no file
- Writing records creates the correct directory structure
- Output file contains the original record, error list, and rejectedAt timestamp
- Filename follows the `{sourceName}_{timestamp}.json` pattern

## Deliverables

| Deliverable | Location | Status |
|-------------|----------|--------|
| `RejectedRecord<T>` model | `src/Core/Conduit.Core/Models/RejectedRecord.cs` | Complete |
| `IRejectedOutputWriter` interface | `src/Core/Conduit.Core/Services/IRejectedOutputWriter.cs` | Complete |
| `JsonRejectedOutputWriter` | `src/App/Conduit/Services/JsonRejectedOutputWriter.cs` | Complete |
| `AppSettings.RejectedOutputDir` | `src/App/Conduit/Models/FeedSettings.cs` | Complete |
| `appsettings.json` update | `src/App/Conduit/appsettings.json` | Complete |
| DI registration | `src/App/Conduit/Services/ServiceCollectionExtensions.cs` | Complete |
| Entry point wiring | `src/App/Conduit/Program.cs`, `Conduit.Worker/Program.cs`, `Conduit.Api/Program.cs`, `Conduit.Cli/Program.cs` | Complete |
| Tests | `tests/Conduit.Tests/JsonRejectedOutputWriterTests.cs` | Complete |

## Acceptance Criteria

- [ ] `dotnet build` passes with no warnings
- [ ] `dotnet test` passes; no existing tests broken
- [ ] Writing a `RejectedRecord` produces a file at `data/rejected/{sourceType}/{sourceName}_{timestamp}.json`
- [ ] Each entry in the output file has `record`, `errors`, and `rejectedAt` fields
- [ ] `data/raw/` output is unchanged when rejected records are present
- [ ] `RejectedOutputDir` reads from `appsettings.json`; changing the value changes the output path
- [ ] `AddConduitPipeline()` registers `IRejectedOutputWriter` — no extra DI calls needed at call sites

## Dependencies

- `TransformedRecord<T>` and `ITransformedOutputWriter` patterns (exist in `Conduit.Core`) — read these before implementing to stay consistent
- `JsonTransformedOutputWriter` (exists in `src/App/Conduit/Services/`) — the implementation to mirror
- ValidationTransform ticket (not yet implemented) — this spec does not wire the rejected tier into the pipeline; that wiring belongs to the ValidationTransform ticket

## Risks

- **`AddConduitPipeline()` signature change breaks entry points** — all four `Program.cs` files call this method; update all of them in the same PR or the build will fail
- **Test isolation** — writer tests create real files; use `Path.GetTempPath()` for output dir and clean up in test teardown, consistent with existing writer tests
