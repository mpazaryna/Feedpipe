# Source Depth: RSS -- Acceptance Scenarios

**PRD:** [prd.md](prd.md)

## Cross-Feed Deduplication

```gherkin
Feature: Content-Similarity Deduplication

  Scenario: Same article from two feeds is deduplicated
    Given the same article appears in two different RSS feeds with different URLs
    When the pipeline processes both feeds
    Then only one copy of the article appears in curated output

  Scenario: Similar titles but different articles are not deduplicated
    Given two articles with similar but distinct titles from different feeds
    When the pipeline processes both feeds
    Then both articles appear in curated output

  Scenario: URL-identical articles are always deduplicated
    Given two items with exactly the same link URL
    When the deduplication stage runs
    Then only the first is kept regardless of title similarity

  Scenario: Similarity threshold is configurable
    Given a similarity threshold is set in configuration
    When two articles exceed the threshold
    Then they are treated as duplicates
```

## Topic Clustering

```gherkin
Feature: Topic Clustering

  Scenario: Related articles are grouped into a cluster
    Given multiple articles about the same topic from different sources
    When the topic clustering stage runs
    Then the articles share a common cluster identifier in their enrichment

  Scenario: Unrelated articles are in separate clusters
    Given articles from unrelated topics
    When the topic clustering stage runs
    Then each article is assigned to a different cluster

  Scenario: Cluster label reflects dominant topic
    Given a cluster of articles about machine learning
    When the clustering stage derives the cluster label
    Then the label reflects the shared topic
```

## Feed Health Tracking

```gherkin
Feature: Feed Health Monitoring

  Scenario: Successful fetch updates last-seen timestamp
    Given a feed that returns items successfully
    When the pipeline runs
    Then the feed's last successful fetch time is recorded

  Scenario: Failed fetch increments error count
    Given a feed that returns a network error
    When the pipeline runs
    Then the feed's error count is incremented
    And the last error message is recorded

  Scenario: Feed health is surfaced in pipeline output
    Given a feed with a non-zero error rate
    When the pipeline runs
    Then the feed health metadata is available in enriched output

  Scenario: Stale feed is flagged
    Given a feed whose last successful fetch was more than 24 hours ago
    When the pipeline runs
    Then the feed is flagged as stale in its health record
```

## Full-Text Extraction

```gherkin
Feature: Full-Text Article Extraction

  Scenario: Summary-only feed item is enriched with full text
    Given a FeedItem whose description contains only a short summary
    When the full-text extraction stage runs
    Then the linked URL is fetched
    And the article body text is added to the enrichment

  Scenario: Extraction failure falls back to feed summary
    Given a FeedItem whose linked URL returns an error
    When the full-text extraction stage runs
    Then the enrichment uses the original feed description as fallback
    And the failure is logged

  Scenario: Items with full description skip extraction
    Given a FeedItem whose description exceeds the summary threshold
    When the full-text extraction stage evaluates the item
    Then no HTTP request is made to the linked URL
```
