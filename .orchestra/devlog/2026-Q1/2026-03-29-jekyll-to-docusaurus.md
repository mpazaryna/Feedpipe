# 2026-03-29: Why Docusaurus, Not Jekyll

## The Problem

After ADR-003 killed the docs site entirely, we reversed course — the `.orchestra/` content (roadmap, PRDs, ADRs, devlogs) is genuinely useful as a browsable site, even if auto-generated API docs aren't. But we needed something that could render `.orchestra/` markdown on GitHub Pages without fighting the tooling.

## The Jekyll Attempt

Jekyll was the obvious first choice: GitHub Pages has native Jekyll support, zero build config needed. We tried three themes in quick succession:

1. **minimal** (`pages-themes/minimal@v0.2.0`) — first attempt. Worked, but the layout was sparse and the whitespace was excessive for dense project docs.
2. **cayman** (`pages-themes/cayman@v0.2.0`) — switched immediately. Better density, but still no dark mode, no built-in search, and the sidebar navigation was nonexistent. Every page was a flat list.
3. **just-the-docs** (`just-the-docs@v0.10.1`) — the best Jekyll theme for docs. Dark mode, search, proper sidebar. But this is where the real problems surfaced.

### Why Jekyll Didn't Work

The core issue: **Jekyll's navigation model doesn't match `.orchestra/`'s structure.**

Jekyll wants front matter in every file to control ordering, titles, and parent-child relationships. Our `.orchestra/` files are plain markdown — no front matter, no Jekyll-specific metadata. To get a proper sidebar with categories (Milestones, Decisions, Devlog, Learning), we'd have needed to either:

- **Inject front matter** into every `.orchestra/` file (polluting the source of truth with rendering concerns), or
- **Generate wrapper files** with front matter that include the originals (fragile, another build step)

Neither option was acceptable. ADR-003's principle is that `.orchestra/` is the source of truth — the site is a read-only view. Jekyll wanted to own the content structure, not just render it.

Additional friction:
- Remote themes load slowly in CI and occasionally fail
- No hot reload for local development without installing Ruby
- Liquid templating errors on markdown that contains `{{ }}` patterns (common in code examples)

## Why Docusaurus Won

Docusaurus solved every Jekyll problem:

1. **Sidebar control is external.** `sidebars.ts` defines the full navigation tree — categories, ordering, collapsed state — without touching the source markdown. The `.orchestra/` files stay clean.

2. **Sync script as the bridge.** A `sync-orchestra.sh` script copies `.orchestra/` content into `docs/docs/` at build time, adding minimal front matter (just `slug` and `title`). The originals are never modified.

3. **Dark mode by default.** No theme hunting.

4. **Local dev actually works.** `npm start` gives you hot reload. No Ruby, no Bundler, no gem version conflicts.

5. **GitHub Pages compatible.** Static HTML output, deployed via the same workflow. The `build` output is plain files, not an SPA (unlike DocFX modern).

### The Trade-off

Docusaurus adds a Node.js build step and an 18K-line `package-lock.json` to the `docs/` folder. For a .NET project, this is a foreign dependency. We accepted it because:

- The build step runs only in CI, not locally (unless you're previewing)
- The `docs/` folder is isolated — it doesn't affect the .NET build
- The alternative (Jekyll) was simpler in theory but harder in practice

## The Full Journey

| Attempt | Tool | Problem |
|---------|------|---------|
| 1 | DocFX legacy | Dated UI, default logo, duplicated IDE docs |
| 2 | DocFX modern | SPA output — GitHub Pages can't serve it |
| 3 | Removed entirely | ADR-003: docs live in-repo only |
| 4 | Jekyll + minimal | Too sparse |
| 5 | Jekyll + cayman | No sidebar, no dark mode, no search |
| 6 | Jekyll + just-the-docs | Navigation requires front matter in every file |
| 7 | **Docusaurus** | External sidebar config, clean sync, dark mode |

Seven attempts to get a docs site right. The lesson: the constraint wasn't "which tool renders markdown" — it was "which tool lets `.orchestra/` stay clean while still producing navigable output." Docusaurus was the first tool that respected that boundary.

## What We Shipped

- `docs/docusaurus.config.ts` — site config with dark mode default
- `docs/sidebars.ts` — explicit sidebar with collapsed categories (Devlog, Learning, Decisions)
- `docs/scripts/sync-orchestra.sh` — copies `.orchestra/` into `docs/docs/` at build time
- `.github/workflows/docs.yml` — CI builds and deploys to GitHub Pages
- Landing page with overview tables for all sections and source adapters
