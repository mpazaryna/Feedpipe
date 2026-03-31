# Spec: ValidationTransform

**PRD:** [prd.md](./prd.md)
**Status:** Complete

## Objective

Add a `ValidationTransform` that runs before deduplication and enrichment, checks each record against source-specific rules, routes invalid records to `IRejectedOutputWriter`, and passes only valid records forward.

## Approach

Follow TDD throughout — write failing tests before each implementation step.

### Step 1: `IRecordValidator` interface in Core

Add `src/Core/Conduit.Core/Services/IRecordValidator.cs`.

Two methods:
- `bool AppliesTo(IPipelineRecord record)` — returns true if this validator handles the given record type
- `IReadOnlyList<string> Validate(IPipelineRecord record)` — returns a list of human-readable error messages; empty list means valid

Keeping this in Core (no adapter dependencies) means the interface is available everywhere without circular references.

### Step 2: `ValidationTransform` in Conduit.Transforms

Add `src/Transforms/Conduit.Transforms/ValidationTransform.cs`.

Implements `ITransform`. Constructor takes:
- `IRejectedOutputWriter rejectedWriter`
- `string sourceType`
- `IEnumerable<IRecordValidator> validators`

`ExecuteAsync` logic:
1. For each record, run all validators where `AppliesTo` returns true
2. Collect all error messages across matching validators
3. Records with errors → create `RejectedRecord<IPipelineRecord>`, add to rejected list
4. Records with no errors → add to valid list
5. After processing all records, if any rejected: call `rejectedWriter.WriteAsync(rejected, sourceType, sourceName)`
6. Return only the valid records

**Note on sourceName:** `ValidationTransform` doesn't have access to `sourceName` — use `sourceType` as the file identifier for rejected output, or pass `sourceName` into the constructor. Pass `sourceName` into the constructor alongside `sourceType`.

### Step 3: `EnrollmentRecordValidator`

Add `src/Transforms/Conduit.Transforms/EnrollmentRecordValidator.cs`.

`AppliesTo`: returns true for `EnrollmentRecord`.

Rules (cast to `EnrollmentRecord` inside `Validate`):
- `SubscriberId` is non-empty
- `MemberName` is non-empty
- `MaintenanceTypeCode` is one of: `021`, `024`, `001`, `025`
- `RelationshipCode` is one of: `18`, `01`, `19`, `20`, `39`, `G8`
- `PlanId` is non-empty
- If `CoverageEndDate` is present, it must be after `CoverageStartDate`
- `CoverageStartDate` must not be more than 1 year in the future

### Step 4: `FeedItemValidator`

Add `src/Transforms/Conduit.Transforms/FeedItemValidator.cs`.

`AppliesTo`: returns true for `FeedItem`.

Rules:
- `Title` is non-empty
- `Link` is non-empty and is a valid absolute URI (`Uri.TryCreate` with `UriKind.Absolute`)
- `Timestamp` (PublishedDate) is not `DateTime.MinValue`

### Step 5: `ResearchRecordValidator`

Add `src/Transforms/Conduit.Transforms/ResearchRecordValidator.cs`.

`AppliesTo`: returns true for `ResearchRecord`.

Rules:
- `Title` is non-empty
- At least one of `Doi` or `Url` is non-empty
- If `Doi` is present, it must match the DOI pattern: starts with `10.` followed by registrant and suffix (regex: `^10\.\d{4,}/.+`)
- If `Abstract` is empty and `AccessLevel` is `Open`, add a warning: "Abstract is empty for an Open access record"

### Step 6: Update `TransformPipeline.CreateForSource`

Update signature in `src/Core/Conduit.Core/Services/TransformPipeline.cs`:

```csharp
public static TransformPipeline CreateForSource(
    ITransformedOutputWriter writer,
    IRejectedOutputWriter rejectedWriter,
    string sourceType,
    string sourceName,
    IEnumerable<IRecordValidator> validators,
    IEnumerable<ITransform> enrichmentTransforms)
```

New stage order:
1. `new ValidationTransform(rejectedWriter, sourceType, sourceName, validators)` ← first
2. `new DeduplicationTransform(writer, sourceType)`
3. Enrichment transforms

### Step 7: Update DI registration

In `src/App/Conduit/Services/ServiceCollectionExtensions.cs`:
- Register `IReadOnlyList<IRecordValidator>` with all three validators (same pattern as enrichment transforms)
- Update XML doc comment on `AddConduitPipeline`

### Step 8: Update call sites of `CreateForSource`

Three files call `CreateForSource` — all need the new signature:

**`src/App/Conduit/Program.cs`**
- Resolve `IRejectedOutputWriter` and `IReadOnlyList<IRecordValidator>` from `provider`
- Pass `source.Name` as `sourceName`

**`src/App/Conduit.Worker/Worker.cs`**
- Add `IRejectedOutputWriter rejectedWriter` and `IReadOnlyList<IRecordValidator> validators` to constructor parameters (primary constructor pattern, same as existing)
- Pass both to `CreateForSource`

**`src/App/Conduit.Api/Program.cs`**
- Add `IRejectedOutputWriter rejectedWriter` and `IReadOnlyList<IRecordValidator> validators` to POST endpoint parameters (same pattern as existing `IOutputWriter writer`)
- Pass both to `CreateForSource`

### Step 9: Tests

Add to `tests/Conduit.Transforms.Tests/`:

- `ValidationTransformTests.cs` — transform routing: valid records pass through, invalid routed to writer, mixed batches handled correctly
- `EnrollmentRecordValidatorTests.cs` — one test per rule, valid and invalid fixture for each
- `FeedItemValidatorTests.cs` — one test per rule
- `ResearchRecordValidatorTests.cs` — one test per rule

Use a mock `IRejectedOutputWriter` (Moq) in transform tests to verify `WriteAsync` is called with the right records.

## Deliverables

| Deliverable | Location | Status |
|-------------|----------|--------|
| `IRecordValidator` interface | `src/Core/Conduit.Core/Services/IRecordValidator.cs` | Complete |
| `ValidationTransform` | `src/Transforms/Conduit.Transforms/ValidationTransform.cs` | Complete |
| `EnrollmentRecordValidator` | `src/Transforms/Conduit.Transforms/EnrollmentRecordValidator.cs` | Complete |
| `FeedItemValidator` | `src/Transforms/Conduit.Transforms/FeedItemValidator.cs` | Complete |
| `ResearchRecordValidator` | `src/Transforms/Conduit.Transforms/ResearchRecordValidator.cs` | Complete |
| `TransformPipeline.CreateForSource` update | `src/Core/Conduit.Core/Services/TransformPipeline.cs` | Complete |
| DI registration | `src/App/Conduit/Services/ServiceCollectionExtensions.cs` | Complete |
| Console entry point | `src/App/Conduit/Program.cs` | Complete |
| Worker | `src/App/Conduit.Worker/Worker.cs` | Complete |
| API entry point | `src/App/Conduit.Api/Program.cs` | Complete |
| Tests | `tests/Conduit.Transforms.Tests/` (4 files) | Complete |

## Acceptance Criteria

- [ ] `dotnet build` passes with no warnings
- [ ] `dotnet test` passes; no existing tests broken
- [ ] A batch with all-valid records returns all records, `WriteAsync` on rejected writer not called
- [ ] A batch with all-invalid records returns empty list, all records appear in `data/rejected/`
- [ ] A mixed batch splits correctly: valid to pipeline output, invalid to rejected
- [ ] `EnrollmentRecord` with unknown `MaintenanceTypeCode` is rejected with a descriptive error
- [ ] `FeedItem` with a relative URL (e.g., `/path/to/article`) is rejected
- [ ] `ResearchRecord` with neither `Doi` nor `Url` is rejected
- [ ] `ValidationTransform` runs before `DeduplicationTransform` in the pipeline

## Dependencies

- `IRejectedOutputWriter` and `RejectedRecord<T>` — already in `Conduit.Core` (complete)
- `JsonRejectedOutputWriter` registered in DI (complete)
- `Conduit.Transforms` project already references adapter projects — validators can reference `FeedItem`, `EnrollmentRecord`, `ResearchRecord` directly

## Risks

- **`CreateForSource` signature change touches three call sites** — update all in one commit or the build fails; compile error will catch any missed site
- **`IRecordValidator` in Core references `IPipelineRecord` only** — validators cast to concrete types inside `Validate`; if a record type is renamed, the cast silently fails and returns no errors. Mitigate: each validator's `AppliesTo` guards the cast, and tests cover both paths
- **`sourceName` not available in `ValidationTransform`** — pass it as a constructor argument alongside `sourceType`; `CreateForSource` receives it from the call site
