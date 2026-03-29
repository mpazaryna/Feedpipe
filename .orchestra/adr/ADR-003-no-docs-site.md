# ADR-003: No External Documentation Site

**Date:** 2026-03-29
**Status:** Active
**Decision:** Conduit does not maintain a generated documentation site. All documentation lives in the repository.

## Context

We tried three documentation site approaches:

1. **DocFX (legacy template)** -- auto-generates API reference from C# XML doc comments. Works on GitHub Pages but uses an outdated template with poor aesthetics and no PDF support.
2. **DocFX (modern template)** -- better UI and PDF support, but outputs a JavaScript SPA that GitHub Pages cannot serve (no static HTML files generated).
3. **Docusaurus** -- excellent docs site with working links and navigation, but cannot auto-generate API reference from C# XML doc comments.

None of these delivered enough value to justify the build step, deployment pipeline, and ongoing maintenance.

## Decision

Documentation lives in the repository, not on a hosted site.

**For agents:** An agent clones the repo and has immediate access to everything -- source code, XML doc comments, `.orchestra/` project docs, README, samples, and output data. A hosted site adds nothing an agent can't get faster from the source.

**For developers in an IDE:** XML doc comments appear as IntelliSense hover tooltips in Visual Studio, Rider, and VS Code. This is where developers actually consume API documentation -- not on a website.

**For humans browsing:** GitHub renders markdown natively. The README, `.orchestra/` devlogs, PRDs, ADRs, and learning notes are all readable directly on GitHub without a build step.

The auto-generated API reference (type signatures, parameter lists) duplicates what the IDE already provides. The project documentation (roadmap, decisions, devlogs) is more useful in the repo where it lives alongside the code it describes.

## Consequences

- The DocFX docs site and GitHub Pages deployment are removed
- The `docs/` folder and `.github/workflows/docs.yml` are deleted
- GitHub Pages is disabled on the repository
- All documentation effort goes into in-repo markdown and XML doc comments
- The README serves as the entry point for both humans and agents
- No documentation build step, no deployment pipeline, no maintenance burden
