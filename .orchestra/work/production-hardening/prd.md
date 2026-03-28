# Production Hardening

**Objective:** Make Feedpipe deployable and operable in a production environment with resilience, observability, and containerized deployment.

## Success Criteria

- [ ] Retry policies with exponential backoff (Polly)
- [ ] Health check endpoints for the Worker and API
- [ ] Structured logging with correlation IDs
- [ ] Metrics/monitoring integration (Application Insights or OpenTelemetry)
- [ ] Dockerfile for each runnable project
- [ ] Docker Compose for local multi-service development
- [ ] Configuration via environment variables (12-factor app)
- [ ] Graceful shutdown handling

## Context

Part of the [Feedpipe Roadmap](../../roadmap.md). The current pipeline works but has no resilience -- a network timeout crashes the worker, there are no health checks, and deployment is manual. This milestone brings the project to production-grade, demonstrating the operational patterns expected in a healthcare data pipeline where reliability and observability are non-negotiable.

## Materials

| Material | Location | Status |
|----------|----------|--------|
| Polly retry policies | src/Feedpipe/Services/ | Not Started |
| Health check endpoints | src/Feedpipe.Api/ , src/Feedpipe.Worker/ | Not Started |
| Correlation ID middleware | src/Feedpipe/ | Not Started |
| OpenTelemetry integration | src/Feedpipe/ | Not Started |
| Dockerfile (API) | src/Feedpipe.Api/Dockerfile | Not Started |
| Dockerfile (Worker) | src/Feedpipe.Worker/Dockerfile | Not Started |
| Docker Compose | docker-compose.yml | Not Started |
| Environment variable config | src/Feedpipe/ | Not Started |
| Resilience tests | tests/Feedpipe.Tests/ | Not Started |

## Notes

This milestone PRD needs to be fleshed out. Run `/orchestra:prd` to expand it when ready.
