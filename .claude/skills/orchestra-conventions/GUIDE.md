# Orchestra Guide

A complete guide to the `.orchestra/` methodology — what it is, why it exists, and how to use every skill in the system.

## What This Is

Orchestra is a methodology for agent-driven project execution. It gives AI agents the context they need to understand, continue, and complete work across sessions — without relying on conversation history, Slack threads, or tribal knowledge.

The core idea: every project is a performance. The performance needs a score. The score is a hierarchy of PRDs. Agents read the score, perform their part, and report back to the conductor when they're done.

## The Problem It Solves

Without orchestra, agent sessions start cold. Every conversation begins with "what are we working on?" and ends with context lost when the session closes. Work planning lives in the human's head. Ticket creation is reactive. Progress tracking is manual.

Orchestra solves this by making project context **file-based, version-controlled, and agent-readable**. An agent opening a project with `.orchestra/` can immediately:

- Read the roadmap to understand the vision
- Find the active milestone to know what matters now
- See what's done and what's not in the materials table
- Pick up a ticket with full traceability to the roadmap

## The Metaphor

The system uses an orchestra metaphor — composer, conductor, orchestra, audience — to define clear roles and ownership boundaries. See [README.md](README.md) for the full breakdown.

## The Document Hierarchy

The system has two distinct layers with different owners:

### Product Layer (owned by the composer/requestor)

- **Roadmap** — The vision. Why this project exists, what milestones matter.
- **PRD** — The intent. What needs to be built, why it matters, what success looks like.

PRDs are refined only with input from the requestor or product team. They answer "why" and "what done looks like" — never "how to build it."

### Execution Layer (owned by the agent)

- **Spec** — The implementation plan. Derived from the PRD, written in terms the agent can execute against.

Specs can be refined by the agent at runtime. If the agent discovers a better approach during implementation, it updates the spec. No human approval needed for spec changes — the PRD's success criteria are the contract.

This separation is deliberate. The requestor should never need to engage with implementation details at the code level. If the agent has questions about purpose or fundamental delivery requirements, it goes back to the PRD. If the agent needs to adjust its approach, it updates the spec.

### The Recursive Structure

```
Roadmap PRD (the score)
└── Milestone PRD (a movement)
    └── Work Item
        ├── PRD (the intent — what and why)
        └── Spec (the plan — how, derived from the PRD)
            └── Deliverables in your repo
```

A PRD already contains the shape of a milestone: an objective, success criteria, a materials table, and references. A roadmap is just the top-level PRD whose materials table lists milestones instead of deliverables. This recursive structure means no new artifact types — just PRDs at every level, with specs as the bridge to execution.

### Work Items

Each ticket gets a folder with a PRD and a spec. These live in different ownership layers:

| Document | Layer | Answers | Refined By |
|----------|-------|---------|------------|
| PRD | Product | What's the goal? Why does it matter? What does success look like? | Requestor / product team only |
| Spec | Execution | How to build it? What steps? What's the approach? | Agent — derived from PRD, updated at runtime |

The PRD is the contract between the requestor and the system. The spec is the agent's working document. An agent that needs to change *what* gets built goes back to the PRD and the requestor. An agent that needs to change *how* it's built updates the spec and keeps going.

### The Materials Table

The key structure that makes everything machine-readable:

```markdown
| Material | Location | Status |
|----------|----------|--------|
| MVP Launch | .orchestra/work/mvp-launch/prd.md | In Progress |
| Beta Testing | .orchestra/work/beta-testing/prd.md | Not Started |
| Public Release | .orchestra/work/public-release/prd.md | Not Started |
```

At the roadmap level, rows are milestones. At the milestone level, rows are deliverables. Tickets are generated from gaps — rows that aren't "Done" become work.

Status values: `Done`, `In Progress`, `Not Started`, `Needs Refresh`, `Cancelled`

## The Folder

See [README.md](README.md) for the full `.orchestra/` folder structure, including ADRs, work items, devlogs, and templates.

---

## The Skills

### Getting Started

#### `/scaffold`

Creates the `.orchestra/` folder structure and walks you through defining the initial roadmap.

**When to use:** Setting up a new project or adding orchestra to an existing one.

**What it does:**
1. Creates the folder structure (adr/, work/, devlog/, templates)
2. Asks for your project vision and milestones
3. Generates a populated roadmap.md
4. Creates milestone PRD stubs
5. Writes an initial devlog entry

**Example:**
```
/scaffold .

> Project name: Chiro App
> Vision: AI-assisted chiropractic SOAP notes that replace paper
> Milestones: MVP (core SOAP flow), Beta (multi-practitioner), Launch (App Store)
```

Result: Fully populated `.orchestra/` with roadmap and 3 milestone stubs ready to flesh out.

---

### The Conductor Loop

The conductor loop is the planning cycle that turns roadmap gaps into executed work:

```
/milestone → /prd → /spec → agentic coding → update ticket → /devlog
```

`/ticket` is optional (pushes to ClickUp for tracking). Each skill is independently useful, but they're designed to chain.

#### `/milestone`

Reviews the active milestone by diffing the materials table against actual repo state.

**When to use:**
- Starting a work session — "what should I work on?"
- Checking progress — "how far along is this milestone?"
- After completing work — "what's left?"

**What it does:**
1. Reads roadmap.md, finds the active milestone
2. Reads the milestone PRD
3. For each materials table row, checks if the deliverable exists and matches its status
4. Surfaces gaps, stale entries, and mismatches
5. Recommends next actions with priority

**Example output:**
```
## Milestone Review: MVP Launch

Progress: 2/5 deliverables done

### Gaps
| Material | Status | Issue |
|----------|--------|-------|
| SOAP note editor | Not Started | Core feature, blocks everything |
| Patient lookup | Not Started | Dependency for SOAP flow |

### Recommended Next Actions
1. SOAP note editor — core to the milestone objective
2. Patient lookup — dependency for #1
```

#### `/prd`

Generates a PRD from a milestone gap.

**When to use:** A milestone review surfaces a gap that needs scoping. The gap is too big or unclear to just start coding.

**What it does:**
1. Reads the milestone PRD and roadmap for context
2. Reads relevant ADRs
3. Asks for objective, success criteria, deliverables, constraints
4. Generates the PRD using the template
5. Saves to `.orchestra/work/{slug}/prd.md`
6. Updates the milestone materials table

**Example:**
```
/prd SOAP note editor

> Objective: Build the core SOAP note editing interface with AI-assisted field population
> Success criteria: Doctor can create, edit, save a SOAP note in under 2 minutes
> Deliverables: SOAPNoteView, SOAPNoteViewModel, NoteService
```

#### `/spec`

Generates an execution spec from an approved PRD. The spec is the bridge between product intent and code — the agent reads the PRD and derives an implementation plan it can execute against.

**When to use:** A PRD is approved and you're ready to define the concrete implementation plan.

**What it does:**
1. Reads the PRD
2. Analyzes the codebase for relevant patterns (if code work)
3. Breaks work into ordered, concrete steps
4. Generates the spec with approach, deliverables, acceptance criteria, risks
5. Saves alongside the PRD

**Example:**
```
/spec soap-note-editor

Generates:
- Step 1: Create SOAPNoteView with section fields
- Step 2: Build SOAPNoteViewModel with validation
- Step 3: Implement NoteService for persistence
- Step 4: Add AI suggestion integration
- Acceptance: Note created in <2min, all fields validate, saves to CloudKit
```

#### `/ticket`

Pushes a work item (PRD or PRD+spec) to ClickUp as a trackable ticket.

**When to use:** A PRD is ready for tracking. Spec is optional — it can be written later.

**What it does:**
1. Reads the PRD (and spec if it exists)
2. Composes a self-contained ticket description
3. Creates the ClickUp ticket via API
4. Renames the work folder to include the ticket ID
5. Updates the milestone materials table with the ClickUp link

**Example:**
```
/ticket soap-note-editor

## Ticket Created
- Title: SOAP Note Editor
- ID: 86e0xyz
- URL: https://app.clickup.com/t/86e0xyz
- Has spec: yes

Ready for `/open 86e0xyz` or `/agent 86e0xyz`
```

---

### Operations

#### `/roadmap`

Read and manage the roadmap directly.

**When to use:**
- **status** — Show the full roadmap with milestone progress roll-up
- **next** — What's the next thing to work on?
- **update** — Mark a milestone or deliverable status
- **add** — Add a new milestone

**Examples:**
```
/roadmap status
/roadmap next
/roadmap update mvp-launch Done
/roadmap add "Analytics Dashboard"
```

#### `/conduct`

The fully autonomous conductor. Runs the entire loop without human checkpoints.

**When to use:** You trust the roadmap and milestones are well-defined, and you want the system to execute autonomously — find gaps, generate PRDs, write specs, create tickets, implement, and update the score.

**What it does:**
1. Finds the active milestone
2. Identifies the highest-priority gap
3. Generates PRD → spec → ticket
4. Implements the work (code or docs)
5. Updates the score
6. Moves to the next gap
7. Stops when the milestone is done or it hits ambiguity

**When NOT to use:**
- Milestones are vague or undefined
- Work requires architectural decisions not captured in ADRs
- You want to review before each step

**Example:**
```
/conduct

[Finds MVP Launch milestone, 3 gaps remaining]
[Generates PRD for "Patient lookup"]
[Writes spec with 4 implementation steps]
[Creates ClickUp ticket 86e0abc]
[Implements: creates PatientLookupView, PatientService, tests]
[Updates milestone: Patient lookup → Done]
[Moves to next gap: "SOAP note templates"]
...
```

---

#### `/devlog`

Writes devlog entries documenting what happened during a work session.

**When to use:** After completing significant work — implementation sessions, architectural decisions, debugging breakthroughs, or any session where context would help future agents or humans understand the trajectory.

**Report types:**

| Type | When to Use |
|------|-------------|
| **Git Journal** | Summarize work from commits — "journal today's work" |
| **Devlog** | Narrative work documentation — "write a devlog" |

**What it does:**
1. Gathers context (git history, milestone state, session work)
2. Generates a structured entry
3. Saves to `.orchestra/devlog/{YYYY-QN}/{YYYY-MM-DD}-{slug}.md`

**Example:**
```
/devlog

Generates a narrative entry capturing:
- What was built or changed
- Decisions made and why
- What's left / next steps
- Lessons learned
```

---

### Background Knowledge

#### `conventions`

Not a slash command — Claude loads this automatically when working in a project with `.orchestra/`. Provides the methodology rules: status flow, when to create ADRs, how to update materials tables, the agent session loop.

---

## Common Workflows

### Starting a New Project

```
/scaffold .
```
Answer the vision and milestone questions. You're ready.

### Daily Work Session

```
/milestone
```
See what's next. Pick a gap. Then:
- `/prd {gap}` → `/spec {name}` → agentic coding → `/devlog`
- `/ticket {name}` optionally to push to ClickUp for tracking

### Weekly Planning

```
/roadmap status
```
Review all milestones. Reprioritize if needed. Add new milestones with `/roadmap add`.

### Closing Work

After implementing, close the loop:
1. Mark deliverable as "Done" in the milestone PRD's materials table
2. If all deliverables done, mark milestone as "Done" in roadmap.md
3. `/devlog` — capture what happened, decisions made, lessons learned

### Going Fully Autonomous

```
/conduct
```
Let the system run. It will stop when it hits ambiguity or completes the milestone.

---

## Principles

1. **The score is the source of truth.** If it's not in roadmap.md or a milestone PRD, it doesn't exist in the system.

2. **PRDs all the way down.** No new artifact types. The same structure at every level keeps the system simple and learnable.

3. **Trace everything back.** Every ticket traces to a spec, every spec traces to a PRD, every PRD traces to a milestone, every milestone traces to the roadmap. If it doesn't trace, it doesn't belong.

4. **Update on close.** An agent that completes work but doesn't update the ticket hasn't finished. The conductor updates the score; the score must reflect reality.

5. **Compose, don't improvise.** The human writes the score. Agents perform it. The separation is what makes autonomous execution safe — agents work within the boundaries the composer defined.

6. **Start simple.** Two milestones and a vision is enough. The system grows as you use it. Over-planning upfront is just another form of procrastination.
