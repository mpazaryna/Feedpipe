# ValidationTransform -- Acceptance Scenarios

**PRD:** [prd.md](prd.md)
**Spec:** [spec.md](spec.md)

## Transform Routing

```gherkin
Feature: Validation Transform Routing

  Scenario: All valid records pass through
    Given a batch of records that all satisfy their validation rules
    When the ValidationTransform runs
    Then all records are returned
    And the rejected writer is never called

  Scenario: All invalid records are rejected
    Given a batch of records that all fail validation
    When the ValidationTransform runs
    Then an empty list is returned
    And all records are written to the rejected tier

  Scenario: Mixed batch splits correctly
    Given a batch containing one valid and one invalid record
    When the ValidationTransform runs
    Then only the valid record is returned
    And the invalid record is written to the rejected tier

  Scenario: ValidationTransform runs before deduplication
    Given a pipeline with validation, dedup, and enrichment stages
    When the pipeline runs
    Then invalid records are rejected before dedup executes
    And the dedup stage never sees invalid records

  Scenario: No validator applies to a record type
    Given a record type with no registered validator
    When the ValidationTransform runs
    Then the record passes through unchanged
```

## EnrollmentRecord Validation

```gherkin
Feature: EDI 834 Enrollment Record Validation

  Scenario: Valid enrollment record passes
    Given a complete EnrollmentRecord with valid codes and dates
    When the EnrollmentRecordValidator runs
    Then no errors are returned

  Scenario: Missing MemberId is rejected
    Given an EnrollmentRecord with an empty MemberId
    When the EnrollmentRecordValidator runs
    Then an error is returned for MemberId

  Scenario: Missing SubscriberId is rejected
    Given an EnrollmentRecord with an empty SubscriberId
    When the EnrollmentRecordValidator runs
    Then an error is returned for SubscriberId

  Scenario: Unknown maintenance type code is rejected
    Given an EnrollmentRecord with MaintenanceTypeCode "999"
    When the EnrollmentRecordValidator runs
    Then an error is returned for MaintenanceTypeCode

  Scenario: Valid maintenance type codes pass
    Given an EnrollmentRecord with MaintenanceTypeCode "021", "024", "001", or "025"
    When the EnrollmentRecordValidator runs
    Then no MaintenanceTypeCode error is returned

  Scenario: Unknown relationship code is rejected
    Given an EnrollmentRecord with RelationshipCode "99"
    When the EnrollmentRecordValidator runs
    Then an error is returned for RelationshipCode

  Scenario: Coverage end date before start date is rejected
    Given an EnrollmentRecord where CoverageEndDate is before CoverageStartDate
    When the EnrollmentRecordValidator runs
    Then an error is returned for CoverageEndDate

  Scenario: Coverage start date more than one year in future is rejected
    Given an EnrollmentRecord with a CoverageStartDate two years from now
    When the EnrollmentRecordValidator runs
    Then an error is returned for CoverageStartDate
```

## FeedItem Validation

```gherkin
Feature: RSS Feed Item Validation

  Scenario: Valid feed item passes
    Given a FeedItem with a non-empty title, absolute URL, and valid timestamp
    When the FeedItemValidator runs
    Then no errors are returned

  Scenario: Empty title is rejected
    Given a FeedItem with an empty title
    When the FeedItemValidator runs
    Then an error is returned for Title

  Scenario: Relative URL is rejected
    Given a FeedItem with a relative URL like "/path/to/article"
    When the FeedItemValidator runs
    Then an error is returned for Link

  Scenario: Missing timestamp is rejected
    Given a FeedItem with Timestamp equal to DateTime.MinValue
    When the FeedItemValidator runs
    Then an error is returned for Timestamp
```

## ResearchRecord Validation

```gherkin
Feature: Zotero Research Record Validation

  Scenario: Valid research record passes
    Given a ResearchRecord with a title and a DOI
    When the ResearchRecordValidator runs
    Then no errors are returned

  Scenario: Missing title is rejected
    Given a ResearchRecord with an empty title
    When the ResearchRecordValidator runs
    Then an error is returned for Title

  Scenario: Record with neither DOI nor URL is rejected
    Given a ResearchRecord with no DOI and no URL
    When the ResearchRecordValidator runs
    Then an error is returned indicating an identifier is required

  Scenario: Malformed DOI is rejected
    Given a ResearchRecord with a DOI that does not start with "10."
    When the ResearchRecordValidator runs
    Then an error is returned for DOI format

  Scenario: Open access record with empty abstract warns
    Given a ResearchRecord marked Open with no abstract
    When the ResearchRecordValidator runs
    Then a warning is returned for the missing abstract
```
