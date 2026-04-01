# Rejected Data Tier -- Acceptance Scenarios

**PRD:** [prd.md](prd.md)
**Spec:** [spec.md](spec.md)

## Rejected Output

```gherkin
Feature: Rejected Record Storage

  Scenario: Invalid records are written to data/rejected/
    Given a record fails validation
    When the pipeline runs
    Then a file appears in data/rejected/{sourceType}/
    And the file contains the rejected record with its error messages

  Scenario: Rejected record includes error details
    Given a record fails validation with two rule violations
    When the rejected writer writes the record
    Then the output contains a "record" field with the original data
    And an "errors" array with both error messages
    And a "rejectedAt" timestamp

  Scenario: Rejected records do not appear in curated output
    Given a batch where some records are invalid
    When the pipeline runs
    Then data/curated/ contains only the valid records
    And data/rejected/ contains only the invalid records

  Scenario: Raw output is unaffected by rejection
    Given a batch where some records are invalid
    When the pipeline runs
    Then data/raw/ contains all original records regardless of validity

  Scenario: No file created when all records are valid
    Given a batch where all records pass validation
    When the pipeline runs
    Then no file is written to data/rejected/

  Scenario: Rejected output directory is configurable
    Given RejectedOutputDir is set to a custom path in appsettings.json
    When a record is rejected
    Then the rejected file is written to the configured path
```

## Filename and Structure

```gherkin
Feature: Rejected File Naming

  Scenario: Rejected file follows standard naming convention
    Given a rejected record for source type "edi834" named "benefits-enrollment"
    When the rejected writer writes the file
    Then the filename matches the pattern "benefits-enrollment_{timestamp}.json"
    And the file is in data/rejected/edi834/

  Scenario: Subdirectory is created on first write
    Given data/rejected/edi834/ does not exist
    When the first rejected record is written for source type "edi834"
    Then the directory is created automatically
```
