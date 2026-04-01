---
name: orchestra-conventions
description: "The .orchestra/ methodology: PRDs all the way down, ADRs, work items, devlogs, and the composer/conductor/orchestra roles. Use when working in a project that has .orchestra/ — provides background knowledge for how agents should interact with the knowledge base."
user-invocable: false
---

# .orchestra/ Conventions

The `.orchestra/` folder is the agent knowledge base — everything an AI agent needs to understand, continue, or reconstruct project context.

## The Hierarchy: PRDs All The Way Down

```
Roadmap PRD (the score)
└── Milestone PRD (a movement)
    └── Deliverables (the notes)
        └── Content files in the repo
```

1. **The roadmap** is `.orchestra/roadmap.md` — the score. Not a work item; the thing that generates work items. One roadmap per project.
2. **Each milestone** is a PRD in `.orchestra/work/{clickup-id}-{name}/prd.md`. Its materials table lists the deliverables needed.
3. **Deliverables** are content files referenced by path in the milestone PRD's materials table.
4. **Tickets** are generated from the gap — materials table rows marked "Not Started" or "Needs Refresh" become tasks.

## The Materials Table

The key structure at every level:

```markdown
| Material | Location | Status |
|----------|----------|--------|
| {name} | {path or link} | Done / In Progress / Not Started / Needs Refresh |
```

Roadmap rows = milestones. Milestone rows = deliverables.

## Roles

**The Composer** (human) writes the score — defines roadmap, sets milestones, decides what "done" looks like. Strategic. Evaluates all inputs before they enter the system. Always human.

**The Conductor** interprets the score — reads active milestone, identifies gaps, directs agents. Starts human, shifts agentic over time as structure improves.

**The Orchestra** (agents) performs — reads the score, picks up tickets, executes work, updates the score. An agent that completes work but doesn't update the PRD hasn't finished.

## Agent Session Loop

1. Read the score (roadmap → active milestone → materials table)
2. Perform (execute the ticket/task)
3. Update the score (mark deliverable status, update milestone progress)

## Folder Structure

```
.orchestra/
├── README.md          ← Explains the folder
├── roadmap.md         ← The score (top-level PRD)
├── adr/               ← Decision Records (long-lived constraints)
├── work/              ← Per-ticket PRDs and specs
│   ├── TEMPLATES/     ← PRD and spec templates
│   └── {id}-{name}/   ← One folder per ticket
└── devlog/            ← Chronological journal by quarter
```

## When to Create What

| Artifact | When | Format |
|----------|------|--------|
| **ADR** | Making a decision future agents must follow | `ADR-{NNN}-{name}.md` |
| **Work item** | Starting a ClickUp ticket | `work/{clickup-id}-{name}/prd.md` |
| **Spec** | Ticket needs execution detail | `work/{clickup-id}-{name}/spec.md` |
| **Devlog** | Something happened worth recording | `devlog/{YYYY}-Q{N}/{date}-{slug}.md` |
| **Roadmap update** | Milestone status changed | Update `roadmap.md` materials table |

## Rules

- **Never overwrite a ADR.** Supersede it with a new ADR that references the old one.
- **Roadmap is singular.** One `roadmap.md` per project, not inside `work/`.
- **Trace everything back.** Tickets → milestone PRD → roadmap. If it doesn't trace, it doesn't belong.
- **Update on close.** Completing work means updating the materials table. The score must reflect reality.

## Comprehensive Guide

For detailed examples, common workflows, and skill-by-skill usage guide, see [GUIDE.md](${CLAUDE_SKILL_DIR}/GUIDE.md).
