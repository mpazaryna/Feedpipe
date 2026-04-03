# Severity Guide

Use these levels when classifying findings in the audit report.

| Severity | Meaning | Examples |
|----------|---------|---------|
| **High** | Security risk or will break in production | Hardcoded secret, no auth on a sensitive endpoint |
| **Medium** | Bad practice that causes operational pain | No health check, hardcoded SKU, no prod config layer |
| **Low** | Missing convention or hygiene | No resource tags, naming inconsistency, unused `UserSecretsId` |
| **Advisory** | Not yet present but will be needed as the project grows | No IaC yet, no App Insights, no CI deploy step |

## Notes

- **Advisory is not a problem** — it means the project is at an early stage, not that something is wrong. Advisory findings are a checklist for future milestones, not action items for today.
- **Absence is a valid finding** — if the project has no Azure SDK references yet, that's "Not Yet Present" in the summary table and an Advisory finding in the details. It keeps the audit useful at any stage of the project.
- **High findings should be addressed before any Azure deployment** — they represent real security or reliability risk in production.
