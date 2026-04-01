---
name: orchestra-gherkin
description: Write and update Gherkin acceptance scenarios in .orchestra/work/{item}/gherkin.md. Use when scoping a new work item, reviewing acceptance criteria before a spec, or documenting what was built after a milestone closes.
argument-hint: "<work-item-name or path>"
---

# Orchestra Gherkin

Write and maintain Gherkin acceptance scenarios in `.orchestra/work/{item}/gherkin.md`. Scenarios define the acceptance bar for a work item — concrete, testable, and unambiguous. They answer the question: how will we know this is done?

## When to Use This Skill

- When scaffolding a new work item (alongside the PRD)
- Before writing a spec — scenarios anchor the acceptance criteria
- After a milestone closes — to document what was actually built
- When a PRD's success criteria are too vague to drive implementation

## Where Scenarios Live

Each work item folder has one `gherkin.md`:

```
.orchestra/work/{item}/
  ticket.md
  prd.md
  gherkin.md    ← this file
  spec.md
```

## Steps

### 1. Read the Work Item Context

From $ARGUMENTS find the work item folder. Read:
- `prd.md` — objective and success criteria
- `ticket.md` — original brief or requirements
- Any referenced ADRs in `.orchestra/adr/`
- `CLAUDE.md` for domain and architectural context

### 2. Identify the Scenarios

For each PRD success criterion, ask: *what would a passing test look like from the outside?*

Group scenarios by feature area. Each Feature block should cover one coherent capability. Common groupings:
- Happy path (the thing works as intended)
- Edge cases (boundary conditions, empty inputs, duplicates)
- Error handling (invalid input, missing config, downstream failures)
- Integration (how this feature interacts with adjacent ones)

### 3. Write the Scenarios

Format:

```gherkin
Feature: {Capability Name}

  Scenario: {Descriptive title — what the user/system does and what happens}
    Given {precondition — state of the world before the action}
    When {action — what is triggered}
    Then {outcome — observable result}
    And {additional outcome if needed}
```

**Rules:**
- `Given` sets up state — it should be achievable in a test fixture
- `When` is a single action — one trigger per scenario
- `Then` describes observable output — file written, value returned, status changed
- Scenarios are independent — no scenario depends on another's side effects
- Use concrete values over abstractions: `"data/raw/rss/"` not `"the output directory"`

### 4. Write the File

File header:

```markdown
# {Work Item Name} — Acceptance Scenarios

**PRD:** [prd.md](prd.md)

## {Feature Area 1}

```gherkin
Feature: ...
  Scenario: ...
```

## {Feature Area 2}

...
```

Save to `.orchestra/work/{item}/gherkin.md`.

### 5. Cross-Check Against PRD

Every PRD success criterion should map to at least one scenario. After writing:
- Read the PRD success criteria
- Confirm each has a corresponding scenario
- Flag any criterion that can't be expressed as a testable scenario — it may need to be rephrased in the PRD

### 6. Scenarios for Closed Milestones

For completed work items, scenarios document what was built — they're retrospective rather than prospective. Read the spec and the implementation, then write scenarios that describe actual behavior. These serve as regression documentation and agent context for future work.

## Scenario Quality Checks

- [ ] Every PRD success criterion has at least one scenario
- [ ] `Given` state is achievable without heroics (real fixtures, not elaborate mocks)
- [ ] `When` is a single action — not "when several things happen"
- [ ] `Then` is observable from outside the component (output file, return value, API response)
- [ ] Scenarios are independent — order doesn't matter, no shared state
- [ ] Concrete values used — file paths, status codes, field names from the actual domain model
- [ ] An agent reading only this file knows exactly what done looks like

## Anti-Patterns to Avoid

| Anti-pattern | Fix |
|---|---|
| `Then the system works correctly` | Specify what correct means — file written, count returned, error raised |
| `Given the developer has set everything up` | Specify the actual precondition — config file exists, source URL is reachable |
| `When many things happen` | Split into multiple scenarios |
| `Scenario: Test feature X` | Name what actually happens — `Scenario: Duplicate records are filtered on second run` |

## Part of Orchestra

| Skill | Purpose |
|-------|---------|
| `orchestra-prd` | Defines the success criteria that Gherkin makes testable |
| `orchestra-gherkin` | Acceptance scenarios — the definition of done |
| `orchestra-spec` | Execution plan — references gherkin as the acceptance bar |
| `orchestra-uml` | Structure diagrams — complements scenarios with visual context |
