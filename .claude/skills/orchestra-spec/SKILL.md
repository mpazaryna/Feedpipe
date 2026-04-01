---
name: spec
description: "Generate an execution spec from an approved PRD — define approach, steps, deliverables, acceptance criteria, and risks. Use when a PRD is approved and the work needs a concrete execution plan."
argument-hint: "<prd-path or work-item-name>"
disable-model-invocation: false
---

# Write Spec

Generate an execution spec from an approved PRD. The spec is the contract between the conductor (you) and the orchestra (agents).

## Prerequisites

- A PRD must exist at `.orchestra/work/{id}-{name}/prd.md`
- The PRD should be approved (user has confirmed objective and success criteria)

## Steps

### 1. Read the PRD

- Find the PRD from $ARGUMENTS (path or work item name)
- Read the PRD completely — objective, success criteria, materials table, context
- Read the parent milestone PRD for broader context
- Read any referenced ADRs

### 2. Analyze the Codebase (if applicable)

If the work involves code changes:
- Glob for relevant files mentioned in the PRD
- Read existing patterns and conventions from CONTEXT.md or CLAUDE.md
- Identify integration points and dependencies
- Note existing tests that may need updating

### 3. Design the Approach

Break the work into concrete steps:
- Each step should be independently executable
- Steps should be ordered by dependency
- Identify which steps could be parallelized
- Note which steps need human input vs. agent execution

### 4. Generate the Spec

Use the template from [references/spec-template.md](${CLAUDE_SKILL_DIR}/references/spec-template.md):

- **Title**: Matches the PRD title
- **PRD link**: Relative path to the PRD
- **Status**: Draft
- **Objective**: Restated from PRD
- **Approach**: Step-by-step execution plan with details
- **Deliverables table**: Concrete output files with paths and status
- **Acceptance criteria**: Testable criteria (derived from PRD success criteria but more specific)
- **Dependencies**: What must exist before each step
- **Risks**: What could go wrong and how to mitigate

### 5. Write the File

Save to: `.orchestra/work/{id}-{name}/spec.md` (alongside the PRD)

### 6. Present for Approval

Show the user the spec. Ask:
- Does the approach make sense?
- Are any steps missing or out of order?
- Are the acceptance criteria specific enough for an agent to verify?
- Should any steps be broken into separate tickets?

## Quality Checks

- [ ] Every PRD success criterion maps to at least one acceptance criterion in the spec
- [ ] Steps are ordered by dependency — no step requires output from a later step
- [ ] Deliverables table has concrete file paths, not vague descriptions
- [ ] Risks have mitigations, not just "this could go wrong"
- [ ] An agent reading only this spec could execute the work without asking questions

## Spec vs. PRD

| | PRD | Spec |
|---|---|---|
| **Answers** | What and why | How |
| **Audience** | Composer (strategic) | Orchestra (execution) |
| **Language** | Business/outcome | Tactical/technical |
| **Approval** | Before spec | Before implementation |
| **Lifespan** | Until milestone closes | Until work is done |
