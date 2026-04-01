# GitHub Journal Entry

Generate a journal entry from git commit history. Just say "journal this" or "journal today's work" and Claude will read recent commits and create a comprehensive entry.

## Trigger Phrases

- "Journal this"
- "Journal this refactor"
- "Journal today's work"
- "Journal the last 4 hours"
- "Journal since this morning"

## Workflow

1. **Read Git Log** - Analyze recent commits (user can specify time range)
2. **Generate Entry** - Write a journal entry from the commit history
3. **Optional Context** - User can add insights beyond what's in commits
4. **Save** - Write to `docs/journal/YYYY-MM-DD-HHMM-slug.md`

## What to Capture

**From git log:**
- Commit messages (the "what")
- Changed files
- Diff stats (lines added/removed)
- Timestamps and authors

**Infer and document:**
- Overall goal/theme of the work
- Technical decisions visible in the commits
- Progression and evolution of the work
- Next steps based on commit patterns

## Git Commands to Run

```bash
# Recent commits (last 24 hours)
git log --since="24 hours ago" --pretty=format:"%h %s" --stat

# Specific time range
git log --since="4 hours ago" --pretty=format:"%H%n%s%n%b%n---" --stat

# Today's commits
git log --since="midnight" --pretty=format:"%h %ad %s" --date=short --stat
```

## Output Format

```markdown
---
date: YYYY-MM-DD
time: HH:MM
slug: brief-description
commits: N
files_changed: N
---

# Journal: [Theme/Goal of Work]

## Summary

[2-3 sentence overview of what was accomplished]

## Work Completed

### [Category 1]
- [Specific change from commit]
- [Another change]

### [Category 2]
- [Change]

## Technical Decisions

- **[Decision]**: [Why, based on commit evidence]

## Files Changed

- `path/to/file.ts` - [what changed]
- `path/to/other.ts` - [what changed]

## Next Steps

- [ ] [Inferred from commit patterns or stated in messages]

---

*Generated from N commits over [time range]*
```

## Example Output

```markdown
---
date: 2025-01-15
time: 14:30
slug: auth-refactor
commits: 7
files_changed: 12
---

# Journal: Authentication System Refactor

## Summary

Refactored the authentication module to use JWT tokens instead of session-based auth. Added refresh token support and updated all API endpoints to use the new middleware.

## Work Completed

### Authentication Core
- Replaced session storage with JWT token generation
- Added refresh token endpoint with 7-day expiry
- Implemented token validation middleware

### API Updates
- Updated all protected routes to use new auth middleware
- Added token refresh logic to API client
- Fixed logout to properly invalidate refresh tokens

## Technical Decisions

- **JWT over Sessions**: Chose JWT for stateless auth to support future microservices architecture (visible in middleware changes)
- **7-day Refresh**: Balanced security with UX based on mobile app requirements (from commit message)

## Files Changed

- `src/auth/jwt.ts` - New JWT generation and validation
- `src/middleware/auth.ts` - Replaced session check with JWT validation
- `src/api/auth.ts` - Added refresh endpoint
- `src/api/client.ts` - Token refresh interceptor

## Next Steps

- [ ] Add rate limiting to auth endpoints
- [ ] Implement token blacklist for logout
- [ ] Update API documentation

---

*Generated from 7 commits over 4 hours*
```

## Requirements

- Must be in a git repository
- Must have commits in the specified time range
- Defaults to last 24 hours if no range specified

## Tips

- Add context for better entries: "Journal this. Key insight: we chose X over Y because..."
- Specify time range for focused entries: "Journal the last 2 hours"
- Use descriptive commit messages for richer journal output
