# 2026-03-29: Dropping the Documentation Site

## What Happened

Removed the hosted documentation site after trying three approaches (DocFX legacy, DocFX modern, Docusaurus) and concluding that none delivered enough value to justify the maintenance.

## Why

The primary consumers of this codebase's documentation are:

1. **AI agents** -- they read source files directly. A hosted HTML site adds nothing.
2. **Developers in IDEs** -- XML doc comments render as IntelliSense tooltips. This is where API docs are actually consumed.
3. **Humans browsing GitHub** -- GitHub renders markdown natively. The README, .orchestra/ docs, and learning notes are all readable without a build step.

A generated docs site solves a problem that doesn't exist for this project. The effort spent wrestling with DocFX templates and GitHub Pages compatibility would be better spent on the pipeline itself.

## What We Tried

- **DocFX legacy template** -- worked on GitHub Pages but looked dated, had the default "D" logo, and the auto-generated API reference duplicated what IDEs already show.
- **DocFX modern template** -- Microsoft's own documentation tool can't deploy to Microsoft's own hosting platform. The modern template outputs an SPA that GitHub Pages can't serve.
- **Docusaurus** -- excellent for markdown docs, zero issues with GitHub Pages, but can't auto-generate from C# XML comments.

## What We Kept

- XML doc comments in every `.cs` file (for IDE IntelliSense)
- `.orchestra/` with roadmap, PRDs, ADRs, devlogs, and learning notes
- `README.md` with badges, project structure, and quick start
- `samples/` and `data/` with real input/output examples

Everything an agent or developer needs is in the repo.

## The Observation

A docs site is an artifact of a world where humans browse documentation in a browser. When the primary consumers are agents reading source code and developers reading tooltips in their IDE, a hosted site becomes maintenance overhead for a use case that barely exists.
