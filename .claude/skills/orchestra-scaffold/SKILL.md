---
name: scaffold
description: "Scaffold the .orchestra/ agent knowledge base in a project — creates folder structure, templates, README, and walks through initial roadmap setup with vision and milestones. Use when setting up a new project or adding .orchestra/ to an existing one."
argument-hint: "<project-path>"
disable-model-invocation: false
---

# Scaffold .orchestra/

Create the `.orchestra/` agent knowledge base structure and set up the initial roadmap interactively.

## What It Creates

```
.orchestra/
├── README.md                          ← Explains the folder to agents and humans
├── roadmap.md                         ← The score — top-level PRD (populated)
├── adr/                               ← Architecture Decision Records
│   └── ADR-000-the-score.md           ← Founding decision: PRDs all the way down
├── work/                              ← Per-ticket work items
│   └── TEMPLATES/
│       ├── prd.md                     ← PRD template
│       └── spec.md                    ← Spec template
└── devlog/                            ← Chronological journal
    └── {YYYY}-Q{N}/                   ← Current quarter folder
```

## Phase 1: Create Structure

1. Determine target path from $ARGUMENTS (default: current working directory)
2. Check if `.orchestra/` already exists — if so, STOP and report what's there
3. Create the directory structure above
4. Generate `README.md` from [references/readme-template.md](${CLAUDE_SKILL_DIR}/references/readme-template.md)
5. Generate `ADR-000-the-score.md` from [references/adr-000-the-score.md](${CLAUDE_SKILL_DIR}/references/adr-000-the-score.md)
6. Generate PRD and spec templates in `work/TEMPLATES/`
7. Create the current quarter devlog folder

## Phase 2: Set Up the Roadmap

The roadmap is the score — every project needs one. Walk the user through defining it.

### Step 1: Ask for the Vision

Ask the user:

```
To set up your roadmap, I need to understand the project:

1. **Project name** — What is this project called?
2. **Vision** — In 1-2 sentences, what does "done" look like at the highest level?
   (Example: "A chiropractic app that replaces paper SOAP notes with AI-assisted documentation")
3. **Who is the audience?** — Who benefits when this is done?
```

Wait for answers before proceeding.

### Step 2: Define Milestones

Ask the user:

```
Now let's break the vision into milestones. A milestone is a meaningful checkpoint —
something you could demo, ship, or celebrate.

What are your first 2-4 milestones? For each one:
- **Name** — short, memorable (e.g., "MVP Launch", "Beta Testing", "API Integration")
- **Objective** — what does "done" look like for this milestone?

Don't overthink it — milestones can be added and refined later.
What matters is having a starting score to play from.
```

If the user is unsure, help them derive milestones from the vision:
- What's the minimum viable version?
- What comes after that?
- What would make it "complete"?

### Step 3: Generate the Roadmap

Create `.orchestra/roadmap.md` as a PRD:

```markdown
# {Project Name} Roadmap

**Objective:** {Vision — 1-2 sentences}

## Success Criteria

- [ ] {Derived from vision — what "fully done" looks like}
- [ ] {Second criterion}
- [ ] {Third criterion}

## Context

{Why this project matters. Who it serves. What problem it solves.}

## Milestones

| Material | Location | Status |
|----------|----------|--------|
| {Milestone 1 name} | .orchestra/work/{slug}/prd.md | Not Started |
| {Milestone 2 name} | .orchestra/work/{slug}/prd.md | Not Started |
| {Milestone 3 name} | .orchestra/work/{slug}/prd.md | Not Started |

## References

- ADR-000: [The Score](.orchestra/adr/ADR-000-the-score.md)
```

### Step 4: Scaffold Milestone PRDs

For each milestone, create a stub PRD at `.orchestra/work/{slug}/prd.md`:

```markdown
# {Milestone Name}

**Objective:** {What "done" looks like for this milestone}

## Success Criteria

- [ ] {To be defined}

## Context

Part of the [{Project Name} Roadmap](../../roadmap.md).

## Materials

| Material | Location | Status |
|----------|----------|--------|
| {To be defined} | | Not Started |

## Notes

This milestone PRD needs to be fleshed out. Run `/orchestra:prd` to expand it when ready.
```

### Step 5: Create Initial Devlog Entry

Write `.orchestra/devlog/{YYYY}-Q{N}/{date}-project-kickoff.md`:

```markdown
# {date}: Project Kickoff

## What Happened
- Scaffolded .orchestra/ agent knowledge base
- Defined project vision: {vision}
- Established {N} initial milestones: {list}

## Decisions
- Using the orchestra methodology (PRDs all the way down)
- See ADR-000 for the founding decision

## Next Steps
- Flesh out the first milestone PRD with `/orchestra:prd`
- Begin work with `/orchestra:milestone` to review gaps
```

## Phase 3: Report

Present everything that was created:

```
## .orchestra/ Scaffolded

**Project:** {name}
**Vision:** {vision}
**Milestones:** {count}

### Created
- .orchestra/README.md
- .orchestra/roadmap.md (populated with {N} milestones)
- .orchestra/adr/ADR-000-the-score.md
- .orchestra/work/TEMPLATES/prd.md
- .orchestra/work/TEMPLATES/spec.md
- .orchestra/work/{slug}/prd.md (×{N} milestone stubs)
- .orchestra/devlog/{quarter}/{date}-project-kickoff.md

### Next Steps
1. Run `/orchestra:milestone` to review the first milestone and identify gaps
2. Run `/orchestra:prd` to flesh out milestone PRDs as you're ready
3. The loop: /orchestra:milestone → /orchestra:prd → /orchestra:spec → /orchestra:ticket → implement → done
```

## Rules

- Always ask for vision and milestones — never scaffold with an empty roadmap
- Milestone slugs use kebab-case
- If the user gives more than 6 milestones, suggest grouping some — too many dilutes focus
- If the user can't articulate a vision, help them — "What would make this project worth celebrating?"
