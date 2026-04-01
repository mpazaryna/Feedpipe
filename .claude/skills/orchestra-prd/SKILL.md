---
name: prd
description: "Generate a PRD from a milestone gap — define objective, success criteria, materials table, and context. Use when a milestone review surfaces a gap that needs scoping before execution."
argument-hint: "<gap-name or description>"
disable-model-invocation: false
---

# Write PRD

Generate a PRD for a work item that traces back to a milestone in the roadmap.

## Prerequisites

- `.orchestra/roadmap.md` must exist
- An active milestone PRD should exist with a gap that motivates this PRD

## Steps

### 1. Establish Context

- Read `.orchestra/roadmap.md` to understand the project vision
- Identify which milestone this PRD serves
- Read the milestone PRD to understand the objective and surrounding deliverables
- Read any relevant ADRs in `.orchestra/adr/`

### 2. Gather Requirements

Ask the user:
- **What is the objective?** What does "done" look like?
- **Why does this matter?** How does it serve the milestone?
- **What are the deliverables?** Files, documents, features, or artifacts
- **What are the constraints?** Technical, timeline, dependencies
- **What are the success criteria?** How do we know it worked?

If the user provides $ARGUMENTS with a description, use that as the starting point and ask only for gaps.

### 3. Generate the PRD

Use the template from [references/prd-template.md](${CLAUDE_SKILL_DIR}/references/prd-template.md):

- **Title**: Clear, specific name
- **Objective**: 1-2 sentences — what "done" looks like
- **Success Criteria**: 3-5 testable checkboxes
- **Context**: Why this matters, what milestone it serves, what problem it solves
- **Materials table**: Each deliverable with location and "Not Started" status
- **References**: Link to milestone PRD, relevant ADRs, external docs
- **Notes**: Constraints, open questions, dependencies

### 4. Determine Work Item ID

- If a ClickUp ticket exists or will be created, use `{clickup-id}-{short-name}`
- If no ticket yet, use a descriptive slug: `{short-name}`
- The user can rename the folder later when a ticket is created

### 5. Write the File

Save to: `.orchestra/work/{id}-{name}/prd.md`

### 6. Update the Milestone

Add or update the row in the parent milestone PRD's materials table:

```markdown
| {PRD title} | .orchestra/work/{id}-{name}/prd.md | Not Started |
```

### 7. Present for Approval

Show the user the complete PRD. Ask:
- Does the objective capture what you want?
- Are the success criteria testable and complete?
- Is anything missing from the materials table?
- Should we proceed to write a spec?

## Quality Checks

- [ ] Objective is specific — not "improve the thing" but "add X that does Y so Z"
- [ ] Success criteria are testable — each one can be checked as done/not done
- [ ] Materials table lists concrete deliverables with file paths
- [ ] Context traces back to a milestone in the roadmap
- [ ] No open questions left unaddressed (flag them if unresolved)
