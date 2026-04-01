# Foundation -- Acceptance Scenarios

**PRD:** [prd.md](prd.md)

## Project Setup

```gherkin
Feature: Developer Onboarding

  Scenario: Clone and run the pipeline end to end
    Given a developer clones the repository
    When they run "dotnet run --project src/App/Conduit"
    Then the pipeline executes without errors
    And output files appear in data/raw/ and data/curated/

  Scenario: Run all tests
    Given a developer clones the repository
    When they run "dotnet test"
    Then all tests pass with no failures
```

## Entry Points

```gherkin
Feature: Multiple Consumption Patterns

  Scenario: One-shot console run
    Given the console runner is started
    When it completes
    Then it ingests all configured sources and exits

  Scenario: Background worker runs on a schedule
    Given the background worker is started
    When the timer interval elapses
    Then the pipeline executes automatically

  Scenario: REST API serves pipeline results
    Given the API is started on localhost:5000
    When a client sends GET /sources
    Then a list of configured sources is returned

  Scenario: CLI lists curated items
    Given the CLI is invoked with the "list" command
    When curated output exists for a source
    Then the items are printed to the console
```

## CI/CD

```gherkin
Feature: Continuous Integration

  Scenario: Build passes on every push
    Given code is pushed to any branch
    When the CI workflow runs
    Then "dotnet build" exits with code 0

  Scenario: Tests run automatically on push
    Given code is pushed to any branch
    When the CI workflow runs
    Then "dotnet test" exits with code 0
    And no test failures are reported
```
