# Azure Audit — Conduit

**Date:** 2026-04-02
**Scope:** All `appsettings*.json`, `*.csproj`, `Program.cs` files, `.github/workflows/`, `.env` presence

---

## Summary

| Area | Status |
|------|--------|
| Secrets & Configuration | Pass |
| Identity & Authentication | Not Yet Present |
| Azure Hosting Config | Findings |
| Infrastructure-as-Code | Not Yet Present |
| .NET / Azure SDK Usage | Not Yet Present |
| CI/CD | Findings |

---

## Findings

### No Azure SDK packages referenced anywhere
**Area:** .NET / Azure SDK Usage
**Severity:** Advisory
**File:** All `*.csproj`

No `Azure.*` or `Microsoft.Azure.*` packages are referenced in any project. The stack is currently `Microsoft.Extensions.*` + Serilog only. This is correct for the current local-filesystem-only pipeline — it becomes relevant when Storage Backends or any Azure-hosted deployment milestone begins.

---

### No health check endpoint on the API
**Area:** Azure Hosting Config
**Severity:** Medium
**File:** `src/App/Conduit.Api/Program.cs`

The API has `/sources`, `/sources/{name}/ingest`, and `/sources/{name}/items` — but no `/health` endpoint. Azure App Service and Container Apps use health probes to determine whether an instance is alive. Without one, the platform has no way to restart an unhealthy instance and load balancers can't route around failures.

---

### No Application Insights or structured logging for Azure
**Area:** Azure Hosting Config
**Severity:** Advisory
**File:** All `Program.cs`

All entry points use Serilog with Console and File sinks. This works locally. In Azure, logs written to the filesystem of a container or App Service instance are ephemeral — they disappear on redeploy or restart. Application Insights (or at minimum structured output that Azure Monitor can ingest) is needed before deploying to Azure. The Serilog foundation is good; adding `Serilog.Sinks.ApplicationInsights` is a small step when the time comes.

---

### No `appsettings.Production.json` in any app project
**Area:** Azure Hosting Config
**Severity:** Advisory
**File:** `src/App/Conduit/`, `src/App/Conduit.Api/`, `src/App/Conduit.Worker/`

Each app has a single `appsettings.json` with local paths (`data/raw`, `data/curated`, `logs/`). There is no production configuration layer. In Azure, these paths would need to point to Azure Blob Storage or mounted volumes, and the configuration would need to come from environment variables or Azure App Configuration — not the committed JSON file. No action needed now, but the gap is worth noting before the first deploy.

---

### CI pipeline has no deployment step
**Area:** CI/CD
**Severity:** Advisory
**File:** `.github/workflows/ci.yml`

The CI workflow builds and tests — nothing more. There is no deploy step, no container build, no Azure deployment. This is appropriate for the current stage. When Azure deployment becomes a goal, the build and deploy steps should be in separate jobs (build produces an artifact; deploy consumes it) and deployment credentials should use OIDC/Workload Identity Federation rather than a stored service principal secret.

---

### `UserSecretsId` present in Worker but not used for sensitive config
**Area:** Secrets & Configuration
**Severity:** Low
**File:** `src/App/Conduit.Worker/Conduit.Worker.csproj:5`

The Worker project has a `UserSecretsId` configured, which is the correct .NET pattern for local development secrets. However, the current `appsettings.json` contains no sensitive values — only local paths and a public RSS URL. This is fine. The `UserSecretsId` is forward-looking infrastructure; just make sure that when real credentials appear (API keys, connection strings), they go into User Secrets locally and environment variables / Key Vault in Azure — never in `appsettings.json`.

---

### No `.env` file present
**Area:** Secrets & Configuration
**Severity:** Pass

No `.env` file exists in the repo. The `.gitignore` previously excluded `.env` (it's still excluded via the dotenv entry). No sensitive values are committed anywhere. Clean.

---

## Learning Notes

### Why a `/health` endpoint matters in Azure

When you deploy to Azure App Service or Container Apps, the platform needs a way to know if your app is actually running and healthy — not just that the process started. It does this by periodically calling a health endpoint you define. If the endpoint stops returning `200 OK`, Azure will restart the instance or stop routing traffic to it. Without one, Azure assumes the app is healthy as long as the process is alive, even if it's deadlocked or unable to handle requests. Adding it is one line: `app.MapGet("/health", () => Results.Ok())`.

### Why filesystem logs disappear in Azure

A container in Azure Container Apps or a slot in App Service has a local filesystem, but it's not persistent — it gets wiped on redeploy, restart, or scale-out. Any Serilog `WriteTo.File` sink writes to that ephemeral disk. When the container restarts, the logs are gone. Application Insights is Azure's native log aggregation service: logs, traces, exceptions, and performance telemetry all flow there and persist independently of the container lifecycle. Adding it to Serilog is a package install and a one-liner in the configuration.

### Why `UserSecretsId` exists and what it's for

.NET's User Secrets system is a local-only mechanism for storing sensitive config values outside the project directory (they go in `~/.microsoft/usersecrets/{id}/secrets.json` on your machine). The `UserSecretsId` in the `.csproj` is the identifier that links the project to its secrets store. It only activates in the Development environment — in Production, the runtime ignores it entirely. Think of it as a personal `.env` file that's scoped to a specific project and guaranteed to never end up committed. When you have a real API key to work with locally, this is where it goes.

### Why OIDC matters more than stored secrets in CI/CD

The old way to deploy from GitHub Actions to Azure was to create a service principal, generate a long-lived client secret, store it in GitHub repository secrets, and use it in the workflow. The problem: that secret has a 1-2 year expiry, needs manual rotation, and if it leaks it can be used from anywhere. OIDC (OpenID Connect) / Workload Identity Federation is the modern replacement — GitHub Actions gets a short-lived token from Azure that's only valid for the duration of that specific workflow run, tied to a specific repo and branch. Nothing to rotate, nothing to leak. When the time comes to wire up Azure deployment, use OIDC rather than a stored secret.

### What `appsettings.Production.json` is for

.NET's configuration system stacks files in order: `appsettings.json` (base) → `appsettings.{Environment}.json` (environment override) → environment variables (highest priority). In Azure, the `ASPNETCORE_ENVIRONMENT` variable is typically set to `Production`, so `appsettings.Production.json` would be loaded automatically. The pattern is: put safe defaults and local paths in `appsettings.json`, put production-specific structure (connection strings, Azure endpoints) as placeholders in `appsettings.Production.json`, and let environment variables or Key Vault inject the real values at runtime. This keeps the shape of production config visible in the repo without committing the values.
