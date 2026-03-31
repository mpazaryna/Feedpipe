# Source Depth: Zotero -- Acceptance Scenarios

**PRD:** [prd.md](prd.md)

## CrossRef Enrichment

```gherkin
Feature: CrossRef API Metadata Resolution

  Scenario: Citation count is resolved for a known DOI
    Given a ResearchRecord with a valid DOI
    When the CrossRef enrichment stage runs
    Then the citation count is added to the enrichment

  Scenario: Publication venue is resolved for a known DOI
    Given a ResearchRecord with a valid DOI
    When the CrossRef enrichment stage runs
    Then the journal or conference name is added to the enrichment

  Scenario: CrossRef returns no result for an unknown DOI
    Given a ResearchRecord with a DOI not found in CrossRef
    When the CrossRef enrichment stage runs
    Then no citation count or venue is added
    And the failure is logged without halting the pipeline

  Scenario: CrossRef API is unavailable
    Given the CrossRef API returns a network error
    When the CrossRef enrichment stage runs
    Then enrichment is skipped for that record
    And the remaining records continue processing
```

## Preprint Linking

```gherkin
Feature: Preprint to Published Version Linking

  Scenario: arXiv preprint is linked to its published version
    Given a ResearchRecord with an arXiv ID that has a known published DOI
    When the preprint linking stage runs
    Then the published DOI is added to the enrichment
    And the record is marked as a preprint

  Scenario: arXiv preprint with no published version
    Given a ResearchRecord with an arXiv ID that has no published DOI
    When the preprint linking stage runs
    Then the record is marked as preprint with no published link

  Scenario: Non-preprint record is not modified
    Given a ResearchRecord with a DOI that is not an arXiv preprint
    When the preprint linking stage runs
    Then the record is unchanged
```

## Collection and Tag Hierarchy

```gherkin
Feature: Zotero Collection and Tag Preservation

  Scenario: Collection path is captured from Zotero metadata
    Given a Zotero CSV entry with a collection path
    When the adapter parses the entry
    Then the collection hierarchy is preserved in the record

  Scenario: Manual tags are captured
    Given a Zotero CSV entry with manual tags
    When the adapter parses the entry
    Then all tags are available on the record

  Scenario: Multiple tags are preserved as a list
    Given a Zotero entry with three manual tags
    When the adapter parses the entry
    Then all three tags appear in the record's tag list
```

## Reading Status

```gherkin
Feature: Reading Status Tracking

  Scenario: Unread status is captured from Zotero metadata
    Given a Zotero entry marked as unread
    When the adapter parses the entry
    Then the record has readingStatus "unread"

  Scenario: Read status is captured
    Given a Zotero entry marked as read
    When the adapter parses the entry
    Then the record has readingStatus "read"

  Scenario: Missing reading status defaults gracefully
    Given a Zotero entry with no reading status metadata
    When the adapter parses the entry
    Then the record has readingStatus "unknown"

  Scenario: Reading status is surfaced in curated output
    Given a ResearchRecord with readingStatus "in-progress"
    When the enrichment stage runs
    Then "readingStatus" appears in the enrichment field of the curated output
```
