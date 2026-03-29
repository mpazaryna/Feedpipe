# 2026-03-29: Pivot to Conduit

## What Happened

Renamed the project from Feedpipe to Conduit and redefined its purpose. What started as an RSS feed pipeline is now a domain-agnostic data processing lab -- a living notebook for .NET pipeline patterns that can be deployed to real environments.

The pivot was driven by realizing the pipeline architecture we built applies far beyond content feeds. The same ingest -> transform -> store pattern handles:

- RSS/Atom content aggregation
- EDI 834 healthcare enrollment data
- Log ingestion and analysis
- Credit card transaction processing
- Research document parsing (arxiv, PubMed)
- PDF extraction pipelines

Keeping a single codebase forces the abstractions to be genuinely pluggable. Each new domain added to the lab stress-tests the architecture and either validates or improves it.

## What Changed

- **Project name:** Feedpipe -> Conduit
- **GitHub repo:** mpazaryna/Feedpipe -> mpazaryna/Conduit
- **All namespaces:** Feedpipe.* -> Conduit.*
- **Multi-Source Ingestion PRD:** Now targets RSS, Atom, and EDI 834 as the three proof-of-concept source types
- **ADR-001:** Documents the domain-agnostic pipeline decision and the separation between shared infrastructure and domain-specific adapters

## The Lab Concept

Conduit is a living notebook -- not a throwaway tutorial and not a production system (yet). It sits in between:

- **Best-practice .NET patterns** are implemented here first, tested, and understood before encountering them in production codebases
- **New source types** are prototyped here to build domain knowledge (e.g., X12 segment parsing for 834) before working on production implementations
- **The codebase is always deployable.** CI runs on every push. Tests pass. Documentation generates. It's not a sketch -- it's working software that happens to also be a learning tool.

The key insight: a well-structured lab teaches more than tutorials because every decision has real consequences. Bad abstractions break the tests. Leaky domain types contaminate other adapters. The code pushes back.

## Decisions

- Conduit is the permanent name -- domain-agnostic, implies data flowing through a channel
- 834 healthcare data is the second source type (after RSS) to prove the adapter pattern works across fundamentally different data shapes
- ADR-001 establishes the rule: pipeline infrastructure is shared, domain types are isolated
- See [ADR-001](../../adr/ADR-001-domain-agnostic-pipeline.md)

## Next Steps

- Write the spec for Multi-Source Ingestion milestone
- Implement ISourceAdapter interface by refactoring RssFeedFetcher
- Add Atom adapter as second content source
- Begin EDI 834 adapter with sample test fixtures
