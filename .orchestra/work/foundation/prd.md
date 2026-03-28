# Foundation

**Objective:** Establish a working pipeline with a clean architecture that teams can extend, test, and deploy with confidence from day one.

## Success Criteria

- [x] A new contributor can clone the repo, run the pipeline, and see results in under 5 minutes
- [x] Changes are validated automatically on every push -- no broken builds reach main
- [x] The codebase is documented well enough that an unfamiliar developer can navigate it without a walkthrough
- [x] The project supports multiple consumption patterns: one-shot run, scheduled background processing, API access, and command-line queries

## Context

Before building advanced features, the pipeline needs a solid foundation. Without clear project structure, automated testing, and CI, every subsequent milestone introduces risk. This milestone de-risks the entire roadmap by ensuring the basics are reliable, documented, and repeatable.

A pipeline that can't be trusted at the foundation level can't be trusted at any level.

## Materials

| Material | Location | Status |
|----------|----------|--------|
| Core shared library | src/Feedpipe.Core/ | Done |
| Console pipeline runner | src/Feedpipe/ | Done |
| Background worker service | src/Feedpipe.Worker/ | Done |
| REST API | src/Feedpipe.Api/ | Done |
| CLI tool | src/Feedpipe.Cli/ | Done |
| Unit tests | tests/Feedpipe.Tests/ | Done |
| CI pipeline | .github/workflows/ci.yml | Done |
| Documentation site | .github/workflows/docs.yml | Done |
| Build conventions | Directory.Build.props, .editorconfig | Done |

## Notes

This milestone is complete. All deliverables shipped and verified.
