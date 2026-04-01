# Source Depth: EDI 834 -- Acceptance Scenarios

**PRD:** [prd.md](prd.md)

## Envelope Tracking

```gherkin
Feature: ISA/GS/ST Envelope Tracking

  Scenario: Interchange control number is captured
    Given an 834 file with an ISA segment
    When the adapter parses the file
    Then the interchange control number from ISA13 is available in the output

  Scenario: Group control number is captured
    Given an 834 file with a GS segment
    When the adapter parses the file
    Then the group control number from GS06 is available in the output

  Scenario: Transaction set ID is captured
    Given an 834 file with an ST segment
    When the adapter parses the file
    Then the transaction set control number from ST02 is available in the output

  Scenario: Multiple transaction sets in one interchange
    Given an 834 file with multiple ST/SE transaction set pairs
    When the adapter parses the file
    Then records from all transaction sets are returned
    And each record is associated with its transaction set ID
```

## Functional Acknowledgments

```gherkin
Feature: Functional Acknowledgment (999/TA1) Generation

  Scenario: Accepted interchange produces a 999 acknowledgment
    Given a valid 834 interchange
    When the adapter processes the file
    Then a 999 acknowledgment is generated with AK1 accepted status

  Scenario: Rejected interchange produces a 999 with error detail
    Given an 834 interchange with a structural error
    When the adapter processes the file
    Then a 999 acknowledgment is generated with AK1 rejected status
    And the error segment and element positions are identified

  Scenario: TA1 generated for ISA-level errors
    Given an 834 file with an invalid ISA segment
    When the adapter processes the file
    Then a TA1 interchange acknowledgment is generated with the error code
```

## Effective Dating

```gherkin
Feature: Overlapping Coverage Period Resolution

  Scenario: Non-overlapping coverage periods for the same member are both kept
    Given two enrollment records for the same member with non-overlapping dates
    When the pipeline processes the records
    Then both records are present in curated output

  Scenario: Overlapping coverage periods are detected
    Given two enrollment records for the same member with overlapping date ranges
    When the effective dating logic runs
    Then a warning is surfaced indicating the overlap
    And the record with the later effective date takes precedence

  Scenario: Superseding enrollment terminates the prior period
    Given an active enrollment record for a member
    And a new enrollment record for the same member with a later start date
    When effective dating logic runs
    Then the prior record's coverage end date is set to the new record's start date minus one day

  Scenario: Termination record closes the active coverage period
    Given an active enrollment record with no end date
    And a termination record with MaintenanceTypeCode "024"
    When effective dating logic runs
    Then the active record is updated with the termination date
```

## X12 Loop Parser

```gherkin
Feature: X12 Loop Structure Parsing

  Scenario: Member-level REF segments are captured
    Given an 834 member loop with multiple REF segments
    When the adapter parses the loop
    Then all REF qualifier/value pairs are available on the record

  Scenario: Subscriber and dependent are linked
    Given an 834 file with a subscriber (INS01=Y) followed by dependents (INS01=N)
    When the adapter parses the file
    Then each dependent's SubscriberId references the preceding subscriber's MemberId

  Scenario: DMG segment captures demographics
    Given an 834 member loop with a DMG segment
    When the adapter parses the loop
    Then date of birth and gender are captured on the record

  Scenario: HD segment captures health coverage detail
    Given an 834 member loop with an HD segment
    When the adapter parses the loop
    Then maintenance type, coverage type, and plan ID are captured
```
