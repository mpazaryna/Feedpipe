# 2026-04-01: Inlining Orchestra Skills from the Marketplace

## What Happened

Moved the orchestra skill suite out of the [agentic-factory marketplace](https://github.com/mpazaryna/agentic-factory) and into this repository directly under `.claude/skills/orchestra-*/`. The skills now live alongside the work they govern.

## Key Decisions

### The visibility problem

Orchestra skills installed from a marketplace are invisible to anyone reading the repo. A contributor — or an agent — cloning Conduit sees the `.orchestra/` folder full of methodology artifacts but has no idea what generates or interprets them. The skills that drive the workflow exist in a separate repository that most readers will never visit.

Inlining the skills closes that gap. The methodology and the tools that apply it now live in the same place. Someone reading `.claude/skills/orchestra-devlog/` understands immediately how devlogs are produced; reading `.claude/skills/orchestra-milestone/` shows how milestone gaps surface and get prioritized.

### Repo ownership over marketplace convenience

The marketplace model makes sense for skills you want to share across many projects or consume without thinking about. Orchestra is different — it's opinionated about *this* project's structure, folder layout, naming conventions, and artifact types. That opinion belongs in the repo, not rented from a shared library.

Inlining also means the skills version with the project. When the methodology evolves — new artifact types, updated templates, revised conventions — the change lands in a commit that also updates the `.orchestra/` content it affects. There's no gap between "what the skill says to do" and "what the repo actually does."

### No changes to methodology, only to location

The move is purely structural. The skill content, templates, and workflows are unchanged. The `.orchestra/` folder layout, artifact naming, and conventions all stay the same. This is a visibility improvement, not a rewrite.

## What We Shipped

- `.claude/skills/orchestra-conventions/` — methodology background: roles, artifact types, the PRD-to-spec-to-implementation loop
- `.claude/skills/orchestra-devlog/` — devlog and git journal workflow with examples
- `.claude/skills/orchestra-milestone/` — milestone review, gap surfacing, next-work proposals
- `.claude/skills/orchestra-prd/` — PRD generation from milestone gaps
- `.claude/skills/orchestra-roadmap/` — roadmap read/update workflow
- `.claude/skills/orchestra-scaffold/` — initial `.orchestra/` setup for new projects
- `.claude/skills/orchestra-spec/` — spec generation from approved PRDs
- `.claude/skills/orchestra-ticket/` — work item scaffolding from a brief or ticket

## What's Next

The skills are in place. The natural next move is to use them: pick one of the three pending Source Depth milestones (EDI 834, RSS, or Zotero), run `/orchestra-milestone` to confirm the gap, then `/orchestra-spec` to produce an execution plan anchored to the gherkin acceptance scenarios already in those folders.
