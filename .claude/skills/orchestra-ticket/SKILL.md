---
name: ticket
description: "Capture a work ticket as the starting point for the orchestra loop — read a brief, scaffold the work item folder, and set up for PRD and spec. Use when starting new work from a ticket, task, or brief."
argument-hint: "<clickup-id, url, or description>"
disable-model-invocation: false
---

# Ticket

Capture a work ticket and scaffold the orchestra work item. The ticket is the contract — the starting point for the loop.

## Flow

```
/ticket → /prd → /spec → implement → /devlog
```

## Steps

### 1. Read the Brief

From $ARGUMENTS, determine the source:

- **ClickUp ID or URL** — fetch the ticket via ClickUp API:
  ```bash
  export CLICKUP_API_KEY=$(grep CLICKUP_API_KEY .env | cut -d '=' -f2)
  curl -s -H "Authorization: $CLICKUP_API_KEY" "https://api.clickup.com/api/v2/task/$TASK_ID"
  ```
- **Text description** — the user is providing the brief directly

Extract: title, objective, any acceptance criteria, priority, context.

### 2. Check for Existing Work Item

Derive the slug: `{clickup-id}-{short-name}` (if from ClickUp) or `{short-name}` (if from text).

Check if `.orchestra/work/{slug}/` already exists.

**If it exists:** show what's there (ticket.md, prd.md, spec.md) and stop. Don't overwrite.

### 3. Scaffold the Work Item

Create the folder and write the ticket file:

```
.orchestra/work/{slug}/
└── ticket.md
```

**ticket.md format:**

```markdown
# {Title}

**Source:** {ClickUp link or "user brief"}
**Priority:** {if known}
**Date:** {today}

## Brief

{The ticket description — what was asked for, in the requestor's words}

## Acceptance Criteria

- [ ] {From the ticket, or "To be defined in PRD"}

## Notes

{Any constraints, context, or references from the original ticket}
```

### 4. Update the Milestone

If an active milestone exists in `.orchestra/roadmap.md`:
- Add a row to the milestone PRD's materials table pointing to the new work item
- If no active milestone, note this in the output — the user can assign it later

### 5. Report

```
## Ticket Captured

- **Title:** {title}
- **Slug:** {slug}
- **Path:** .orchestra/work/{slug}/ticket.md
- **Milestone:** {milestone name or "unassigned"}

### Next Steps
1. Run `/orchestra:prd {slug}` to expand the brief into a full PRD
2. Run `/orchestra:spec {slug}` to plan execution
3. Implement and log with `/orchestra:devlog`
```

## Rules

- Never overwrite an existing work item — check first, always
- The ticket.md preserves the original brief — don't rewrite or improve it
- Every ticket gets a PRD and spec downstream — keep the folder ready for both
- If the brief is vague, capture it as-is — the PRD step is where clarity happens
