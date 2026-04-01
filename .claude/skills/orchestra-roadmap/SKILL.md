---
name: roadmap
description: "Read and manage the .orchestra/roadmap.md — show status, identify active milestones, find gaps, propose next work. Use when the user asks about project status, what to work on next, or wants to update the roadmap."
---

# Roadmap Manager

Read and manage the project's `.orchestra/roadmap.md` — the score.

## Commands

Parse $ARGUMENTS to determine the action:

- **status** — Show current roadmap state with milestone progress
- **next** — Identify the active milestone and what's not done
- **update** — Mark a milestone or deliverable as done/in-progress
- **add** — Add a new milestone to the roadmap

## Status

1. Read `.orchestra/roadmap.md`
2. Parse the materials table
3. For each milestone row, read its PRD and parse its materials table
4. Present a roll-up:

```
## Roadmap: {Project Name}

### {Milestone 1} — {status}
  - {deliverable}: {status}
  - {deliverable}: {status}
  Progress: N/M done

### {Milestone 2} — {status}
  ...
```

## Next

1. Find the first milestone that isn't "Done"
2. Read its PRD's materials table
3. List items marked "Not Started" or "Needs Refresh"
4. Suggest which to tackle based on dependencies and priority

## Update

1. Identify the target (milestone or deliverable) from $ARGUMENTS
2. Update the status in the appropriate materials table
3. If all deliverables in a milestone are "Done", update the milestone status in roadmap.md

## Add

1. Ask for: milestone name, objective, initial deliverables
2. Create `.orchestra/work/{id}-{name}/prd.md` using the PRD template
3. Add a row to `roadmap.md` materials table

## Rules

- Always read the current state before modifying
- Trace changes: deliverable → milestone → roadmap
- Never remove rows — mark as "Cancelled" if abandoned
