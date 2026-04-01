# UML in the Orchestra Methodology

## A Brief History

UML — the Unified Modeling Language — was standardized by the Object Management Group in 1997, consolidating a decade of competing notations from Booch, Rumbaugh, and Jacobson into a single visual language for software design. It arrived at the height of the object-oriented movement and became the lingua franca of enterprise software architecture through the 2000s.

Then the industry moved on. Agile pushed back against upfront design. Diagrams fell out of fashion. Keeping them in sync with code felt like busywork. By the 2010s, most teams had stopped drawing them entirely.

## Why UML Matters Again

The return of UML isn't nostalgia — it's a practical response to a new constraint: **agents don't read code the way humans do.**

A human engineer navigating an unfamiliar codebase builds a mental model incrementally — following call stacks, tracing data flow, forming intuitions from variable names and patterns. An agent working in a context window does something different: it reads what it's given, reasons from it, and acts. If the context contains only raw code, the agent's understanding is bounded by what it can infer from syntax. If the context also contains a sequence diagram, a component map, and a class hierarchy, the agent starts with a structural understanding that would otherwise take many file reads to reconstruct — if it reconstructed it correctly at all.

UML diagrams are **compressed, high-signal representations of design intent**. They communicate in seconds what the code communicates in minutes, and they communicate things the code doesn't communicate at all: why components are separated the way they are, what the intended runtime sequence is, which interfaces are the extension points.

## UML as an Orchestra Artifact

In the orchestra methodology, every artifact has a role. PRDs capture intent. Specs capture approach. ADRs capture decisions. Devlogs capture what happened. UML diagrams capture **structure** — the shape of the system at a point in time.

They belong in `.orchestra/uml/` for the same reason the other artifacts do: so that any participant in the project — human or agent — can orient themselves quickly. A conductor reading the score before orchestrating a milestone should be able to look at the component diagram and understand the architecture without reading thirty files. A developer onboarding to the project should be able to see the pipeline flow before writing a line of code.

Mermaid is the format of choice here because it renders natively in GitHub and Docusaurus, lives as plain text alongside the code, and can be updated in the same commit as the feature that changes the design. A diagram that can't be kept in sync with the code is a liability. One that lives in the repo and renders in the docs site is an asset.

## Folder Structure

```
.orchestra/uml/
  sequence/     — runtime behavior: how components interact over time
  component/    — structural relationships: layers, dependencies, boundaries
  class/        — type hierarchy: interfaces, models, extension points
```

Add new diagram types as the project grows. The convention is one `.md` file per diagram, with the Mermaid block embedded directly.
