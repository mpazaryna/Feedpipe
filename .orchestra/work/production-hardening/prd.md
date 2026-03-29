# Production Hardening

**Objective:** Make Conduit reliable enough to run unattended in production -- where failures are recovered from automatically, problems are visible before users notice, and deployment is repeatable.

## Success Criteria

- [ ] Transient failures (network timeouts, DNS blips, API rate limits) are retried automatically without human intervention
- [ ] Operators can determine pipeline health at a glance without reading logs
- [ ] A single request can be traced end-to-end across all pipeline stages
- [ ] The pipeline can be deployed to any container host with a single command
- [ ] Configuration works identically across local development and production environments
- [ ] The pipeline shuts down gracefully without losing in-flight work

## Context

A pipeline that works on a developer's machine is not the same as a pipeline that works in production. The difference is what happens when things go wrong -- and things always go wrong. Networks drop. APIs rate-limit. Disks fill up. Containers get killed.

Without resilience, a single failed HTTP request can crash the worker and stop all processing. Without observability, operators don't know it crashed until downstream consumers complain. Without containerization, every deployment is a manual, error-prone process.

This milestone closes the gap between "it works" and "it runs."

## Materials

| Material | Location | Status |
|----------|----------|--------|
| Retry policies | src/Conduit/Services/ | Not Started |
| Health check endpoints | src/Conduit.Api/, src/Conduit.Worker/ | Not Started |
| Request correlation | src/Conduit/ | Not Started |
| Observability integration | src/Conduit/ | Not Started |
| Dockerfiles | src/Conduit.Api/, src/Conduit.Worker/ | Not Started |
| Docker Compose | docker-compose.yml | Not Started |
| Environment-based config | src/Conduit/ | Not Started |
| Resilience tests | tests/Conduit.Tests/ | Not Started |

## Notes

This milestone PRD needs a spec before implementation. Run `/orchestra:spec` when ready.
