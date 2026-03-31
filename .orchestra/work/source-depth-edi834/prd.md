# PRD: Source Depth: EDI 834

**Milestone:** Source Depth: EDI 834
**Status:** Not Started

## Objective

Deepen the EDI 834 adapter from a working prototype to a more complete X12 implementation — one that handles the structural and temporal complexity found in real-world benefit enrollment files.

## Success Criteria

- [ ] Transaction and batch envelopes (ISA/GS/ST) are tracked and surfaced in pipeline output
- [ ] Functional acknowledgment generation (999/TA1) is supported so the adapter can signal acceptance or rejection of an interchange
- [ ] Overlapping coverage periods are detected and resolved using effective dating logic
- [ ] The X12 loop parser handles real-world 834 structures beyond the basic subscriber segment
- [ ] All new behavior is covered by tests and documented

## Context

The current 834 adapter successfully parses subscriber records and produces enrollment output. But real-world 834 files from clearinghouses and payers carry richer structure: ISA/GS/ST envelopes that define transaction sets, functional acknowledgment expectations, and member records with overlapping or superseding coverage periods. Without handling these, the adapter is credible as a lab experiment but not representative of what an enterprise integration would need to deal with.

This milestone brings the 834 adapter closer to that bar — not by implementing a full X12 EDI stack, but by addressing the structural and temporal gaps most likely to appear in files from actual trading partners.

## Materials

| Deliverable | Location | Status |
|-------------|----------|--------|
| PRD | `.orchestra/work/source-depth-edi834/prd.md` | Complete |
| Spec | `.orchestra/work/source-depth-edi834/spec.md` | Not Started |

## References

- Milestone: [Source Depth: EDI 834](.orchestra/roadmap.md)
- Gap analysis: ClickUp 86e0mhaeb
- ADR-001: [Domain-Agnostic Pipeline](../../adr/ADR-001-domain-agnostic-pipeline.md)
