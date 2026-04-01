---
name: devlog-documentation
description: Guide for creating high-quality technical devlog documentation that captures important development learnings, patterns, and architectural decisions. Use when documenting breakthrough moments, complex features, or knowledge that will be valuable for future reference.
license: MIT
---

# Technical Devlog Documentation Guide

## Overview

This guide helps you create clear, comprehensive technical documentation for your devlog. Great devlog documentation serves as both historical record and future reference, capturing the "why" and "how" of important development decisions and learnings.

## When to Create Devlog Documentation

✅ **Create documentation when:**
- You solve a complex problem or overcome a significant challenge
- You discover an important pattern or architectural approach
- You implement a feature with non-obvious design decisions
- You learn something that will save time in the future
- You make architectural decisions that others (or future you) need to understand
- You create a reusable pattern or abstraction

❌ **Don't document:**
- Trivial code changes or simple bug fixes
- Temporary workarounds or hacks
- Information already well-documented elsewhere
- Implementation details that are self-explanatory from code

## Document Structure

### 1. Title and Location

Start with a clear, descriptive title and file location:

```markdown
# Feature Name / Pattern Name

**Location:** `path/to/relevant/file.swift`
```

**Tips:**
- Use descriptive titles (not "Notes" or "Documentation")
- Include file paths for code-related docs
- Use singular form ("Feature Flag" not "Feature Flags System")

### 2. Overview Section

Provide a high-level summary in 1-3 sentences:

```markdown
## Overview

[Concise explanation of what this is and why it exists]
```

**Good example:**
> "Feature flags control which features are visible in different environments without requiring separate build configurations."

**Bad example:**
> "This document describes the feature flag system we built."

### 3. Core Concepts Section

Define key concepts, enums, or data structures:

```markdown
## Distribution Environments

```swift
public enum Distribution {
    case dev          // Development environment
    case testflight   // Beta testing environment
    case appstore     // Production environment
}
```

**Automatic Detection:**
- `.dev` - Default for all development builds
- `.testflight` - Requires `TESTFLIGHT` compilation flag
```

**Tips:**
- Include actual code snippets where relevant
- Add inline comments in code
- Explain how things are detected or initialized
- Use tables for comparing different states

### 4. Detailed Explanation

Break down the feature into logical sections:

```markdown
## Current Feature Flags

### `showLibrary: Bool`

Controls visibility of the Library feature:

| Environment | Visible? | Rationale |
|------------|----------|-----------|
| dev | ✅ Yes | Developer tool |
| testflight | ❌ No | Not relevant |
```

**Tips:**
- Use subsections (###) for each major component
- Include "what" and "why" for each element
- Use tables for comparing states or options
- Add emojis sparingly for visual scanning (✅❌⚠️)

### 5. Usage Examples

Show real-world usage with code:

```markdown
## Using Feature Flags

### In Views

```swift
struct SomeView: View {
    @Environment(\.featureFlags) private var flags

    var body: some View {
        if flags.showLibrary {
            LibraryButton()
        }
    }
}
```
```

**Best practices:**
- Provide 2-3 concrete examples
- Show the most common use cases
- Include context (comments, surrounding code)
- Use realistic variable names

### 6. Advanced Topics or Patterns

Document advanced usage or important patterns:

```markdown
## Testing Different Distributions Locally

You can test distributions in development using overrides:

```swift
Distribution.override = .testflight
```

**Benefits:**
- ✅ Verify before shipping
- ✅ No need for separate schemes
```

**Tips:**
- Explain non-obvious techniques
- Call out benefits with checkmarks
- Include warnings or gotchas
- Show alternative approaches if applicable

### 7. Step-by-Step Guides

For processes or workflows, use numbered steps:

```markdown
## Adding New Feature Flags

### 1. Add Property with Default

```swift
public let showNewFeature: Bool = true
```

**Why defaults?** Existing code won't break.

### 2. Add to init() Switch

```swift
case .dev:
    self.showNewFeature = true
```

### 3. Use in Views

```swift
if flags.showNewFeature {
    NewFeatureView()
}
```
```

**Tips:**
- Number major steps clearly
- Include code for each step
- Explain "why" in callout boxes
- Keep steps focused and actionable

### 8. Patterns and Examples

Show real-world patterns or timelines:

```markdown
## Gradual Rollout Pattern

Feature flags enable trunk-based development:

**Example Timeline:**

```swift
// Week 1: Develop in dev
case .dev:
    self.showLiquidGlass = true

// Week 2: Beta test
case .testflight:
    self.showLiquidGlass = true  // ← Enable

// Week 3: Ship to production
case .appstore:
    self.showLiquidGlass = true  // ← Launch
```
```

**Tips:**
- Show evolution over time when relevant
- Use comments with arrows (← →) to highlight changes
- Provide concrete timelines or milestones
- Include rationale for each stage

### 9. Best Practices Section

List do's and don'ts:

```markdown
## Best Practices

### ✅ Do

- Add feature flags for incomplete features
- Use descriptive flag names
- Test all modes before shipping
- Remove flags after rollout

### ❌ Don't

- Don't leave override uncommented
- Don't use flags as permanent config
- Don't accumulate too many flags
- Don't forget to update all distributions
```

**Tips:**
- Split into Do and Don't sections
- Use ✅ and ❌ for visual clarity
- Keep items concise (1 line each)
- Focus on common mistakes or gotchas

### 10. Related Documentation

Link to related resources:

```markdown
## Related Documentation

- **Architecture:** `docs/architecture/core-patterns.md`
- **Issue Tracking:** GitHub issue #123
- **Strategic Plan:** `docs/plans/roadmap.md`

## Inspiration

Based on [Article Title](https://url.com), adapted for this project.
```

**Tips:**
- Link to related docs in your project
- Credit original sources or inspiration
- Include issue numbers for tracking
- Link to strategic plans or roadmaps

## Code Formatting Guidelines

### Code Blocks

Use language-specific syntax highlighting:

````markdown
```swift
let example = "Use language identifier"
```

```bash
# Shell commands
npm install package
```

```json
{
  "config": "value"
}
```
````

### Inline Code

Use backticks for:
- File paths: `path/to/file.swift`
- Variable names: `showLibrary`
- Function calls: `init()`
- Enum cases: `.dev`
- Class names: `FeatureFlags`

### Comments in Code

Add helpful comments in code examples:

```swift
case .dev:
    self.showLibrary = true  // ← Add cases

// Week 2: Beta test with users
case .testflight:
    self.showLibrary = true  // Enable for feedback
```

**Comment styles:**
- `// ← Highlight important lines` - Use arrows
- `// Week 2: Context` - Add timeline/phase context
- `// Not ready yet` - Explain decisions

## Table Usage

### When to Use Tables

✅ **Good for:**
- Comparing states or options
- Feature availability across environments
- Enum cases with descriptions
- Configuration options

❌ **Avoid for:**
- Long explanations (use paragraphs)
- Code examples (use code blocks)
- Sequential steps (use numbered lists)

### Table Patterns

**Comparison Table:**
```markdown
| Environment | Visible? | Rationale |
|------------|----------|-----------|
| dev | ✅ Yes | Developer tool |
| testflight | ❌ No | Not relevant |
```

**Reference Table:**
```markdown
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| showLibrary | Bool | false | Controls library visibility |
```

## Visual Elements

### Emojis (Use Sparingly)

- ✅ ❌ - Do's and don'ts
- ⚠️ 🚨 - Warnings or important notes
- 💡 - Tips or insights
- 📝 - Notes or reminders
- 🎯 - Goals or objectives

### Callout Boxes

Use bold labels for callouts:

```markdown
**Benefits:**
- Faster development
- Better testing

**Remember:** Comment out overrides!

**Why this works:** Explanation here.
```

### Arrows and Indicators

```swift
case .dev:
    self.showFeature = true  // ← Enable here

// ↓ This leads to
// ↑ This came from
// → Next step
// ← Previous context
```

## Document Length Guidelines

### Short Docs (< 100 lines)
- Quick reference for simple patterns
- Single feature or component
- Minimal sections needed

### Medium Docs (100-300 lines)
- Most common devlog length
- Complete feature documentation
- Multiple sections with examples

### Long Docs (> 300 lines)
- Comprehensive system documentation
- Multiple related components
- Consider splitting into multiple docs

## Writing Style

### Voice and Tone

- **Clear and direct:** Avoid unnecessary words
- **Present tense:** "Controls visibility" not "Will control"
- **Active voice:** "Add the flag" not "The flag should be added"
- **Conversational:** Write like you're explaining to a teammate

### Sentence Structure

**Good:**
> "Feature flags control which features are visible."

**Bad:**
> "The feature flag system is a mechanism that can be utilized to control the visibility of various features."

### Technical Precision

- Use exact type names: `Bool` not "boolean"
- Include full paths: `src/core/config/flags.swift`
- Use proper case: `FeatureFlags` not "feature flags class"
- Be specific: "TestFlight environment" not "testing"

## Common Patterns

### Problem → Solution Pattern

```markdown
## Background

We needed to [problem description].

## Solution

We implemented [solution] which [benefits].

## Implementation

[Code and details]
```

### Concept → Usage Pattern

```markdown
## Overview

[What it is]

## How It Works

[Core concepts]

## Using [Feature]

[Examples and usage]
```

### Evolution Pattern

```markdown
## Initial Approach

We started with [approach].

## Problems

This caused [issues].

## Current Solution

We now use [better approach].
```

## Maintenance

### Updating Documentation

- Update docs when behavior changes
- Add new examples as they emerge
- Remove outdated information
- Archive deprecated patterns

### Archiving Old Docs

When features are removed:

```markdown
# [ARCHIVED] Old Feature

**Status:** Deprecated in v2.0.0, removed in v3.0.0

[Original documentation preserved below]
```

## Template

Here's a starter template:

```markdown
# [Feature/Pattern Name]

**Location:** `path/to/file.swift`

## Overview

[1-3 sentence summary of what this is and why it exists]

## Core Concepts

[Define key types, enums, or data structures]

## Usage

### [Common Use Case 1]

```swift
// Code example
```

### [Common Use Case 2]

```swift
// Code example
```

## Advanced Topics

[Non-obvious patterns or techniques]

## Best Practices

### ✅ Do

- [Best practice 1]
- [Best practice 2]

### ❌ Don't

- [Common mistake 1]
- [Common mistake 2]

## Related Documentation

- **Related Doc:** `path/to/doc.md`
```

## Examples from Real Projects

### Feature Flags Example

See the provided Feature Flags documentation for an excellent example that:
- Starts with clear overview
- Defines core concepts with code
- Shows usage in multiple contexts
- Includes step-by-step guides
- Provides best practices
- Links related documentation

### Other Good Patterns

**State Management:**
- Document state flow
- Show initialization
- Explain mutations
- Include diagrams if complex

**API Integrations:**
- Document endpoints
- Show request/response examples
- Include error handling
- List rate limits or constraints

**Architecture Decisions:**
- Explain the problem
- List alternatives considered
- Show chosen solution
- Justify the decision

## Quick Reference

| Section | Required? | Purpose |
|---------|-----------|---------|
| Title + Location | ✅ Yes | Identify what and where |
| Overview | ✅ Yes | High-level summary |
| Core Concepts | ✅ Yes | Define key elements |
| Usage Examples | ✅ Yes | Show how to use |
| Best Practices | ⚠️ Recommended | Prevent mistakes |
| Related Docs | ⚠️ Recommended | Cross-reference |
| Advanced Topics | ❌ Optional | Complex scenarios |
| Patterns/Timelines | ❌ Optional | Evolution or workflows |

## Tools and Tips

### For Claude Code Users

When documenting with Claude Code:

1. **Start with context:** Share relevant code files
2. **Ask for structure:** "Help me document [feature]"
3. **Iterate:** Refine sections one at a time
4. **Review:** Check for accuracy and completeness
5. **Commit:** Save docs alongside code changes

### Markdown Editors

- VS Code with Markdown Preview
- Obsidian for linked note-taking
- Typora for WYSIWYG editing
- GitHub/GitLab for web viewing

### Version Control

- Commit docs with related code changes
- Use descriptive commit messages
- Tag major doc revisions
- Keep docs in sync with code

## Inspiration and Resources

- **Writing Style:** [Google Developer Documentation Style Guide](https://developers.google.com/style)
- **Markdown:** [CommonMark Spec](https://commonmark.org/)
- **Code Examples:** [Effective Documentation Examples](https://documentation.divio.com/)

## Next Steps

- Review your existing docs using this guide
- Create a template for your project
- Share this guide with your team
- Iterate and improve based on feedback
