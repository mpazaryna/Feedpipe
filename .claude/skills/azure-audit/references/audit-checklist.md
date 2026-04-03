# Azure Audit Checklist

Use this checklist when running Step 2 of the audit. For each area, evaluate what's present and flag absences as "Not Yet Present" — absence is a valid finding.

---

## Secrets and Configuration

- [ ] No connection strings or secrets hardcoded in source files
- [ ] No secrets committed in `appsettings.json` (real values, not placeholders)
- [ ] Sensitive config uses environment variables or Azure Key Vault references (`@Microsoft.KeyVault(...)`)
- [ ] `.env` is in `.gitignore`

---

## Identity and Authentication

- [ ] Azure SDK clients use `DefaultAzureCredential` (not connection strings with keys where avoidable)
- [ ] No storage account keys or SAS tokens hardcoded
- [ ] Managed Identity is used (or planned) rather than service principal secrets where possible

---

## Configuration for Azure Hosting

- [ ] `appsettings.json` has an `appsettings.Production.json` or environment-variable overrides for prod values
- [ ] Logging is configured for Azure (Application Insights SDK present, or structured logging that Azure can consume)
- [ ] Health check endpoints exist (`/health`) for App Service or Container Apps

---

## Infrastructure-as-Code (if Bicep/Terraform present)

- [ ] Resource names follow a consistent naming convention (e.g., `{type}-{app}-{env}-{region}`)
- [ ] SKUs are parameterized, not hardcoded (dev vs. prod can use different tiers)
- [ ] Resources use tags (at minimum: `environment`, `project`)
- [ ] No hardcoded passwords or keys in IaC files
- [ ] Outputs are used to pass values between modules rather than hardcoding resource names

---

## .NET / Azure SDK Usage

- [ ] Azure SDK clients are registered as singletons or scoped in DI (not `new`-ed per request)
- [ ] `Azure.*` package versions are current (check `.csproj` references)
- [ ] Retry and resilience policies are configured for Azure clients (transient fault handling)

---

## CI/CD (if pipelines present)

- [ ] Secrets are stored in GitHub Actions secrets or Azure Key Vault, not in workflow YAML
- [ ] Deployment uses OIDC / Workload Identity Federation rather than long-lived service principal secrets where possible
- [ ] Build and deploy steps are separated
