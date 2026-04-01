---
name: orchestra-uml
description: Create and update Mermaid UML diagrams in .orchestra/uml/ — sequence, component, and class diagrams. Use when adding new diagrams, updating existing ones after architecture changes, or reviewing diagram coverage for a milestone.
argument-hint: "<diagram-type> <subject> or <work-item-name>"
---

# Orchestra UML

Create and maintain Mermaid UML diagrams in `.orchestra/uml/`. Diagrams are compressed, high-signal context artifacts — they communicate structure and runtime behavior to both humans and agents faster than code alone.

## When to Use This Skill

- After a new source adapter, transform, or interface is added
- When a milestone introduces a new component or changes an existing boundary
- When an agent or developer will need to understand the system without reading dozens of files
- When a spec requires a current-state or future-state architecture view

## Diagram Types

| Type | Folder | What It Shows |
|------|--------|--------------|
| **Sequence** | `.orchestra/uml/sequence/` | Runtime behavior — how components interact over time, message order, stage sequencing |
| **Component** | `.orchestra/uml/component/` | Structural relationships — layers, project dependencies, interface implementations |
| **Class** | `.orchestra/uml/class/` | Type hierarchy — interfaces, models, envelopes, extension points |

One `.md` file per diagram. Mermaid block embedded directly in the file.

## Size Constraint: Keep Diagrams Browser-Renderable

**Diagrams must render cleanly in a browser for human-in-the-loop review.** This is a hard constraint, not a preference.

Mermaid diagrams that are too large fail to render, become unreadable at normal zoom, or time out. The rule:

- **Sequence diagrams:** max ~15 participants and ~20 messages. If more are needed, split by sub-flow (e.g., `pipeline-ingestion.md`, `pipeline-transform.md` rather than one monolithic `pipeline-run.md`).
- **Component diagrams:** max ~12 nodes. If the full architecture is larger, create focused sub-diagrams by layer or concern.
- **Class diagrams:** max ~8 classes per diagram. Group by interface cluster, not "all types in the system."

If a subject can't be captured within these limits, **split into multiple focused diagrams** rather than cramming everything into one. A diagram that renders is infinitely more useful than a complete diagram that doesn't.

> The existing `.orchestra/uml/` diagrams follow this pattern — see `sequence/pipeline-ingestion.md`, `sequence/pipeline-transform.md`, and `sequence/pipeline-run.md` as the model for how to decompose a large flow into composable sub-diagrams.

## Steps

### 1. Identify What's Needed

From $ARGUMENTS determine:
- Which diagram type (sequence / component / class)
- What subject (pipeline run, a specific adapter, the full type hierarchy, etc.)
- Whether you're creating new or updating existing

Check `.orchestra/uml/` for existing diagrams before creating new ones.

### 2. Read the Relevant Context

- Read `CLAUDE.md` for architecture overview and project layout
- Read relevant source files (interfaces, adapters, transforms) in `src/`
- Read existing diagrams in the target folder to match style and scope
- Read `.orchestra/uml/README.md` for the rationale and conventions

### 3. Choose the Right Mermaid Diagram Type

**Sequence diagram** — use `sequenceDiagram`:
```
sequenceDiagram
    participant Builder
    participant Agent
    Builder->>Agent: Launch with context
    Agent-->>Builder: ready_for_review
```

**Component diagram** — use `graph TD` or `graph LR`:
```
graph TD
    A[Conduit.Core] --> B[Conduit.Sources.Rss]
    A --> C[Conduit.Transforms]
```

**Class diagram** — use `classDiagram`:
```
classDiagram
    class IPipelineRecord {
        +string Id
        +string SourceType
        +DateTime IngestedAt
    }
    IPipelineRecord <|-- RssItem
```

### 4. Write the Diagram File

File naming: `{subject}.md` — lowercase, hyphenated, descriptive.

File structure:
```markdown
# {Title}

Brief one-sentence description of what this diagram shows and why it exists.

```mermaid
{diagram content}
```

## Notes

- Any non-obvious decisions or constraints captured here
- What is intentionally omitted and why
```

Save to the correct subfolder under `.orchestra/uml/`.

### 5. Check for Coverage Gaps

After writing, ask:
- Does a sequence diagram exist for the main pipeline run? (`sequence/pipeline-run.md`)
- Does a component diagram show the full project structure? (`component/architecture.md`)
- Does a class diagram cover the core interfaces and extension points? (`class/domain-model.md`)

If any are missing and the work item touches those areas, flag it.

### 6. Update on Architecture Changes

When a milestone changes the architecture, the relevant diagrams must update in the same commit. A diagram that diverges from the code is worse than no diagram — it actively misleads agents and developers.

## Quality Checks

- [ ] Diagram renders without syntax errors (Mermaid syntax is valid)
- [ ] Diagram is within size limits (sequence ≤15 participants/20 messages, component ≤12 nodes, class ≤8 classes)
- [ ] Diagram shows what the title and description claim it shows — no more, no less
- [ ] Nothing in the diagram contradicts the actual code
- [ ] Labels use the same names as the code (interface names, class names, method names)
- [ ] File is in the correct subfolder (sequence / component / class)
- [ ] If split from a larger diagram, cross-references to sibling diagrams are noted in the Notes section

## Part of Orchestra

| Skill | Purpose |
|-------|---------|
| `orchestra-conventions` | Methodology and roles |
| `orchestra-spec` | Execution plan — often references UML artifacts |
| `orchestra-uml` | Structure and runtime diagrams |
| `orchestra-gherkin` | Acceptance scenarios |
