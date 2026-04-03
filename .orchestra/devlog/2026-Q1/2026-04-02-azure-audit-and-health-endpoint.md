# 2026-04-02: Azure Audit and Health Endpoint

## What Happened

Created an `azure-audit` skill and ran it against the codebase. One finding was immediately actionable — a missing `/health` endpoint on the API. Added the endpoint, wrote tests for it using `WebApplicationFactory`, and wired the new test project into the solution.

## The Azure Audit

The audit scans all Azure-touching surface area: configuration files, `.csproj` package references, `Program.cs` DI registration, CI/CD pipelines. It produces two outputs: findings with severity levels and a learning section that explains each finding in plain terms.

Running it now — before any Azure work has started — was intentional. The project has no Bicep, no Azure SDK references, no deployment pipeline. Most findings came back "Not Yet Present" or Advisory, which is exactly right for this stage. The audit becomes a pre-flight checklist for when Azure work begins, not a retrospective after mistakes are already in production.

The one finding worth acting on immediately: no `/health` endpoint.

## The Health Endpoint Decision

Azure App Service and Container Apps use health probes to determine whether an instance is alive and should receive traffic. Without one, the platform can't distinguish a deadlocked app from a healthy one — it just watches the process. Adding `/health` is a one-line change that costs nothing now and prevents a real operational gap later.

```csharp
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("Health");
```

Returns `200 OK` with `{ "status": "healthy" }`. Simple and sufficient for a health probe.

## Testing the API with WebApplicationFactory

The existing test projects are all unit tests or adapter-level integration tests. Testing an HTTP endpoint requires a different approach: `WebApplicationFactory<Program>` spins up the full ASP.NET pipeline in-memory, no actual server needed.

This required three things:

1. **Expose `Program` as a partial class** — Minimal API `Program.cs` files are implicitly `internal`. The test factory needs a type reference, so adding `public partial class Program { }` at the bottom of `Program.cs` makes it accessible without changing anything about how the app runs.

2. **New test project** — `tests/Conduit.Api.Tests/` with `Microsoft.AspNetCore.Mvc.Testing`. Same xUnit + pattern as the rest of the test suite.

3. **`GlobalUsings.cs`** — the existing test projects use a `GlobalUsings.cs` for `global using Xunit;` rather than relying on implicit usings. The new project follows the same convention.

Two tests: status is `200 OK`, body contains `"healthy"`. Both pass.

## The `Conduit.slnx` Discovery Issue

Running `dotnet test` at the root without specifying a project uses the solution file for discovery. The new test project wasn't showing up because it hadn't been added to `Conduit.slnx`. Added it to the `/tests/` folder in the solution — all 154 tests now run together from a single `dotnet test` invocation.

## What We Deferred

Several audit findings were deliberately left alone:

- **Application Insights** — the Serilog foundation is right; adding an Azure sink before there's an Azure environment to point at is premature wiring.
- **`appsettings.Production.json`** — the production config shape belongs with the deployment milestone, not before it.
- **`UserSecretsId` in Worker** — already in place; nothing to do until real secrets appear.
- **IaC and CI deploy step** — these belong to a future Azure deployment milestone.

The audit report lives at `.orchestra/audits/azure-audit-2026-04-02.md` and will serve as the checklist when those milestones start.

## What We Shipped

- `.claude/skills/azure-audit/SKILL.md` — audit skill: scans Azure surface area, produces findings + learning notes, saves to `.orchestra/audits/`
- `.orchestra/audits/azure-audit-2026-04-02.md` — first audit run
- `src/App/Conduit.Api/Program.cs` — `/health` endpoint + `public partial class Program {}`
- `src/App/Conduit.Api/Conduit.Api.csproj` — suppressed CS1591 warning on `Program` partial class
- `tests/Conduit.Api.Tests/Conduit.Api.Tests.csproj` — new test project
- `tests/Conduit.Api.Tests/HealthCheckTests.cs` — 2 tests via `WebApplicationFactory`
- `tests/Conduit.Api.Tests/GlobalUsings.cs` — `global using Xunit;`
- `Conduit.slnx` — new test project registered

## What's Next

The remaining audit findings are all contingent on Azure work beginning. The natural next move is to pick one of the four open roadmap milestones — Source Depth (EDI 834, Zotero, or RSS) or Storage Backends — and start there.
