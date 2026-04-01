---
name: devlog
description: Developer reporting for the orchestra methodology. Write devlogs and git journals. Devlogs live in .orchestra/devlog/. Use when writing journals, devlogs, or documenting development work.
---

# Devlog

Document development work. From git commits to narrative logs. Output goes to `.orchestra/devlog/`.

## Report Types

| Type | When to Use | Trigger Phrases |
|------|-------------|-----------------|
| **Git Journal** | Summarize work from commits | "journal this", "journal today's work" |
| **Devlog** | Narrative work documentation | "devlog", "write a devlog" |

## How to Use This Skill

1. **Identify the report type** from the request
2. **Load the appropriate template** from `${CLAUDE_SKILL_DIR}/examples/`:
   - Git journal → Read `${CLAUDE_SKILL_DIR}/examples/github-journal.md`
   - Devlog → Read `${CLAUDE_SKILL_DIR}/examples/devlog.md`
3. **Follow the workflow** in that template
4. **Generate the report** in the specified format

## Quick Reference

| Request | Template |
|---------|----------|
| "Journal this", "journal the last 4 hours" | `${CLAUDE_SKILL_DIR}/examples/github-journal.md` |
| "Write a devlog", "devlog update" | `${CLAUDE_SKILL_DIR}/examples/devlog.md` |

## Report Purposes

### Git Journal
Generates structured journal entries from git commit history. Great for:
- End of day summaries
- Sprint retrospectives
- Documenting refactors or features

### Devlog
Narrative-style work logs with context and decisions. Great for:
- Explaining technical decisions
- Sharing learnings with the team
- Building institutional knowledge

## Part of Orchestra

| Skill | Purpose |
|-------|---------|
| `orchestra:conventions` | Methodology and roles |
| `orchestra:roadmap` | Roadmap management |
| `orchestra:milestone` | Milestone progress |
| `orchestra:devlog` | Work documentation and communication |
