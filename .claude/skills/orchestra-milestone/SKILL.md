---
name: milestone
description: "Review milestones and project progress — diff materials table against repo state, surface gaps, and propose next work items. Use when the user asks about milestones, project status, what to work on next, what's left to do, or wants to check progress on the roadmap."
argument-hint: "<milestone-name or 'active'>"
---

# Milestone Review

Read the active milestone PRD, compare its materials table against actual repo state, and surface what needs attention.

## Steps

### 1. Find the Active Milestone

- Read `.orchestra/roadmap.md`
- Parse the materials table
- Find the first milestone with status "In Progress" or "Not Started"
- If $ARGUMENTS specifies a milestone name, use that instead
- Read the milestone's PRD at the path in the materials table

### 2. Audit the Materials Table

For each row in the milestone PRD's materials table:

**If location is a file path:**
- Check if the file exists
- If it exists, read it and assess completeness
- Compare actual state to the status column — flag mismatches

**If location is a ClickUp link:**
- Note it for the user (don't fetch unless clickup skills are installed)

**If status is "Done":**
- Verify the deliverable actually exists and looks complete

**If status is "Not Started" or "Needs Refresh":**
- This is a gap — candidate for new work

### 3. Present the Review

```
## Milestone Review: {milestone name}

**Objective:** {from PRD}
**Progress:** {N}/{M} deliverables done

### Gaps (work needed)
| Material | Status | Issue |
|----------|--------|-------|
| {name} | Not Started | {why it matters} |
| {name} | Needs Refresh | {what changed since last version} |

### Stale (status says done but reality differs)
| Material | Claimed | Actual |
|----------|---------|--------|
| {name} | Done | {what's actually wrong} |

### On Track
| Material | Status |
|----------|--------|
| {name} | Done |
| {name} | In Progress |

### Recommended Next Actions
1. {highest priority gap — why}
2. {second priority — why}
3. {third — why}
```

### 4. Propose Work

For each gap, suggest whether it needs:
- A **PRD** (if scope is unclear or multi-deliverable)
- A **spec** (if approach needs defining)
- A **ticket** (if ready to execute)
- **Direct work** (if small enough to just do)

## Rules

- Read-only — don't modify any files
- Be honest about mismatches — if status says "Done" but the file is empty, flag it
- Trace everything back to the roadmap — gaps should connect to the milestone objective
