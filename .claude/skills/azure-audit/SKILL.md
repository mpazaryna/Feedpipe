---
name: azure-audit
description: Audit the codebase for Azure best practices and produce findings + learning notes. Use when you want to understand the current Azure posture of the project, catch mistakes before they surface in a team review, or build intuition about .NET-to-Azure patterns.
---

# Azure Audit

Review the codebase for Azure best practices. Produces two outputs: an **audit report** of findings and a **learning section** that explains each finding in plain terms for a developer building .NET/Azure intuition.

## What This Skill Does

Scans all Azure-touching surface area in the codebase:
- Configuration files (`appsettings.json`, `appsettings.*.json`, `.env`)
- Infrastructure-as-code (`*.bicep`, `*.tf`, `*.bicepparam`)
- Application code using Azure SDKs (`Azure.*`, `Microsoft.Azure.*`, `Microsoft.Extensions.Azure`)
- CI/CD pipelines (`.github/workflows/`, `azure-pipelines.yml`)
- Connection strings, secrets, and credentials handling
- Dependency injection and service registration

For each area it checks for best practices, flags gaps, and explains why each finding matters.

## Steps

### 1. Discover Azure Surface Area

Glob for all relevant files:
- `**/*.bicep`, `**/*.tf`, `**/*.bicepparam`
- `**/appsettings*.json`
- `**/*.csproj` (look for Azure SDK package references)
- `.github/workflows/**`
- `**/Program.cs`, `**/ServiceCollectionExtensions.cs` (DI registration)
- `.env`, `*.env`

Read each file. Build a map of what Azure services and patterns are present.

### 2. Run the Audit Checks

Load the full checklist from [references/audit-checklist.md](${CLAUDE_SKILL_DIR}/references/audit-checklist.md). Work through each area. Note: **absence is also a finding** — if the project is heading toward Azure but key practices aren't in place yet, flag them as "Not Yet Present."

Areas covered by the checklist:
- Secrets & Configuration
- Identity & Authentication
- Configuration for Azure Hosting
- Infrastructure-as-Code (if Bicep/Terraform present)
- .NET / Azure SDK Usage
- CI/CD (if pipelines present)

### 3. Produce the Audit Report

Format:

```markdown
# Azure Audit — {Project Name}

**Date:** {date}
**Scope:** {what was scanned}

## Summary

| Area | Status |
|------|--------|
| Secrets & Configuration | {Pass / Findings / Not Yet Present} |
| Identity & Authentication | {Pass / Findings / Not Yet Present} |
| Azure Hosting Config | {Pass / Findings / Not Yet Present} |
| Infrastructure-as-Code | {Pass / Not Yet Present} |
| .NET / Azure SDK Usage | {Pass / Findings / Not Yet Present} |
| CI/CD | {Pass / Findings / Not Yet Present} |

## Findings

### {Finding Title}
**Area:** {area}
**Severity:** {High / Medium / Low / Advisory}
**File:** `{path}:{line if relevant}`

What was found and what it should be instead.

---
```

See [references/severity-guide.md](${CLAUDE_SKILL_DIR}/references/severity-guide.md) for severity level definitions.

### 4. Produce the Learning Section

After the findings, add a learning section. For each finding, write a plain explanation:

```markdown
## Learning Notes

### Why DefaultAzureCredential matters

[2-4 sentences explaining the concept in terms a .NET developer new to Azure will understand.
Connect it to something familiar where possible. Explain the consequence of getting it wrong
in production, not just "it's best practice".]

### Why SDK clients should be singletons in DI

...
```

Keep each note focused and direct. The goal is to build intuition, not reproduce the docs.

### 5. Save the Report

Save to: `.orchestra/audits/azure-audit-{YYYY-MM-DD}.md`

Create the `.orchestra/audits/` folder if it doesn't exist.

Present a summary to the user and ask if they want to address any findings now.

## What "Not Yet Present" Means

This project may not have Azure infrastructure yet. That's expected — flag what's missing as advisory findings so the audit is useful when Azure work begins, not just after it's already wired in. The audit should be runnable at any stage of the project.

## Part of Orchestra

| Skill | Purpose |
|-------|---------|
| `azure-audit` | Azure best practices review + learning notes |
| `orchestra-spec` | Execution plan for addressing audit findings |
| `orchestra-devlog` | Document what was learned and changed |
