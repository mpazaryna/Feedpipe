# ADR-003: Documentation Site Serves .orchestra/ Content Only

**Date:** 2026-03-29
**Status:** Accepted
**Decision:** The docs site renders .orchestra/ markdown via Docusaurus on GitHub Pages. No auto-generated API reference. The source of truth is always .orchestra/ -- the site is a read-only view.

## Context

We tried three documentation site approaches:

1. **DocFX (legacy template)** -- auto-generates API reference from C# XML doc comments. Works on GitHub Pages but uses an outdated template with poor aesthetics and no PDF support.
2. **DocFX (modern template)** -- better UI and PDF support, but outputs a JavaScript SPA that GitHub Pages cannot serve (no static HTML files generated).
3. **Docusaurus** -- excellent docs site with working links and navigation, but cannot auto-generate API reference from C# XML doc comments.

Docusaurus was the best fit: it renders `.orchestra/` markdown with proper navigation and sidebar labels, deploys as static HTML to GitHub Pages, and doesn't require auto-generated API docs.

## Decision

The documentation site is a **read-only view** of `.orchestra/` content, built with Docusaurus and deployed to GitHub Pages.

- A sync script (`docs/scripts/sync-orchestra.sh`) copies `.orchestra/` markdown into Docusaurus at build time
- The CI workflow (`.github/workflows/docs.yml`) runs the sync, builds, and deploys on push to `main`
- No auto-generated API reference -- IDE IntelliSense and XML doc comments serve that role
- The source of truth is always `.orchestra/`; the site is a convenience view

**For agents:** An agent clones the repo and has immediate access to everything -- source code, XML doc comments, `.orchestra/` project docs, README, samples, and output data.

**For developers in an IDE:** XML doc comments appear as IntelliSense hover tooltips in Visual Studio, Rider, and VS Code.

**For humans browsing:** The Docusaurus site provides a navigable, searchable view of project docs (roadmap, ADRs, devlogs, PRDs). GitHub also renders the markdown natively for those who prefer it.

## Consequences

- The `docs/` folder contains the Docusaurus site configuration
- `.github/workflows/docs.yml` deploys to GitHub Pages on every push to `main`
- All documentation effort goes into `.orchestra/` markdown and XML doc comments
- The Docusaurus site is generated, never hand-edited -- changes go through `.orchestra/`
- The README serves as the entry point for both humans and agents
