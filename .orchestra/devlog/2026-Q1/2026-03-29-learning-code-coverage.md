# Learning: Code Coverage as a Development Practice

How to measure, report, and think about test coverage in .NET.

## Why Coverage Matters

Coverage isn't a vanity metric. It answers one question: **if I refactor this code, will the tests catch a regression?** Lines with zero coverage are lines where bugs hide undetected.

But 100% coverage isn't the goal either. Testing DI wiring, framework boilerplate, and Program.cs entry points adds noise without catching real bugs. The target is high coverage on code that makes decisions (adapters, parsers, transformations) and low ceremony everywhere else.

## How Coverage Works in .NET

The coverage tool instruments the compiled code, runs the tests, and records which lines were hit. The output is a Cobertura XML file that tools can render into reports.

### Running locally

```bash
# Collect coverage data
dotnet test --collect:"XPlat Code Coverage"

# Generate an HTML report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report"

# Open in browser
open coverage-report/index.html
```

The HTML report shows:
- Overall coverage percentage per assembly
- Line-by-line highlighting (green = covered, red = not covered)
- Branch coverage (did both sides of an if/else get tested?)

### Running in CI

Our CI workflow collects coverage on every push and PR:

1. `dotnet test --collect:"XPlat Code Coverage"` -- generates Cobertura XML
2. `reportgenerator` -- converts XML to a GitHub-flavored Markdown summary
3. The summary is published to the GitHub Actions job summary page
4. The full report is uploaded as a downloadable artifact

This means every PR shows its coverage impact. No surprises.

## Coverage Targets

For a pipeline project like Conduit:

| Layer | Target | Why |
|-------|--------|-----|
| Adapters (parsers, API calls) | 90%+ | This is where bugs live -- malformed input, missing fields, network errors |
| Models (records, enums) | 100% | They're simple and every property should be exercised by some test |
| Output writers | 80%+ | File I/O has edge cases (permissions, disk full) but some are hard to test |
| Program.cs / DI wiring | 0% | Framework plumbing -- tested implicitly by integration tests |
| Worker/API handlers | Low | Thin wrappers around adapter + writer -- tested via adapter tests |

The real metric isn't the number. It's this: **can you change the internals of an adapter and trust the test suite to tell you if you broke something?**

## What Not to Do

- Don't chase 100% coverage by testing getters, constructors, and framework code
- Don't write tests that just exercise lines without asserting meaningful behavior
- Don't ignore coverage on new code -- if you add a feature, add tests
- Don't let coverage decline silently -- CI makes it visible on every PR

## Comparing to Python

| Python | .NET |
|--------|------|
| `pytest --cov` | `dotnet test --collect:"XPlat Code Coverage"` |
| `pytest-cov` (plugin) | Built into .NET SDK (no extra package in test projects) |
| HTML report via `coverage html` | HTML report via `reportgenerator` |
| `.coveragerc` for config | `runsettings` file (optional) |
| Coverage shown in terminal | Coverage published to GitHub Actions summary |

The concepts are identical. The tooling differs but the output is the same: a line-by-line map of what's tested and what isn't.
