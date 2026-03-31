# Data Transformation -- Acceptance Scenarios

**PRD:** [prd.md](prd.md)
**Spec:** [spec.md](spec.md)

## Deduplication

```gherkin
Feature: Deduplication Transform

  Scenario: Duplicate records within a run are filtered
    Given two records with the same dedup key in the same batch
    When the deduplication stage runs
    Then only the first record is kept
    And the second is discarded

  Scenario: Duplicate records across runs are filtered
    Given a record was written to data/curated/ in a previous run
    When the same record appears in the current run
    Then the deduplication stage filters it out

  Scenario: Unique records pass through
    Given a batch of records with distinct dedup keys
    When the deduplication stage runs
    Then all records pass through unchanged

  Scenario: EDI 834 uses composite dedup key
    Given two EnrollmentRecords with the same subscriber ID and plan
    But different coverage start dates
    When the deduplication stage runs
    Then both records are kept

  Scenario: EDI 834 composite key collision
    Given two EnrollmentRecords with the same subscriber ID, plan, and start date
    When the deduplication stage runs
    Then only the first record is kept
```

## Enrichment

```gherkin
Feature: Content Enrichment

  Scenario: RSS items receive keyword extraction
    Given a FeedItem with a title and description
    When the RSS enrichment stage runs
    Then the transformed record includes an "keywords" field
    And the original record fields are unchanged

  Scenario: EDI 834 records receive enrollment status
    Given an EnrollmentRecord with maintenance type code "024"
    When the EDI 834 enrichment stage runs
    Then the transformed record includes enrollmentStatus "terminated"

  Scenario: EDI 834 active enrollment status
    Given an EnrollmentRecord with maintenance type code "021" and no end date
    When the EDI 834 enrichment stage runs
    Then the transformed record includes enrollmentStatus "active"

  Scenario: Zotero records receive domain tags
    Given a ResearchRecord with an abstract
    When the Zotero enrichment stage runs
    Then the transformed record includes derived domain tags

  Scenario: Enrichment does not modify the original record
    Given a pipeline record
    When any enrichment stage runs
    Then the "record" field in the output matches the original exactly
    And enriched fields appear only in the "enrichment" field
```

## Output Tiers

```gherkin
Feature: Raw and Curated Output

  Scenario: Raw output is written before transformation
    Given the pipeline runs with a source
    When ingestion completes
    Then a file appears in data/raw/{sourceType}/

  Scenario: Curated output is written after transformation
    Given the pipeline runs with a source
    When transformation completes
    Then a file appears in data/curated/{sourceType}/

  Scenario: Raw output is preserved when records are deduplicated
    Given a run produces duplicates that are filtered
    When the run completes
    Then data/raw/ contains all original records
    And data/curated/ contains only the deduplicated set

  Scenario: Pipeline is idempotent
    Given the pipeline is run N times on the same input
    When deduplication is active
    Then data/curated/ contains the same records as after the first run
```

## Pipeline Composition

```gherkin
Feature: Composable Transform Pipeline

  Scenario: Stages execute in order
    Given a pipeline with validation, dedup, and enrichment stages
    When the pipeline runs
    Then validation runs first
    Then deduplication runs second
    Then enrichment runs last

  Scenario: Empty input passes through all stages
    Given an empty list of records
    When the pipeline runs
    Then an empty list is returned with no errors

  Scenario: Adding a new stage requires no core changes
    Given a developer implements ITransform
    And registers it in DI
    When the pipeline runs
    Then the new stage executes without changes to existing stages
```
