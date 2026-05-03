---
name: git-review
description: Review current staged/unstaged changes for bugs, security issues, and logic errors. Use when the user wants a code review before committing, asks "is this safe to commit", or types /git-review.
---

# review — AI diff review

1. Run `git diff HEAD 2>/dev/null || git diff --cached` to get the diff. If empty, tell the user there are no changes.
2. Review the diff. Focus on:
   - **Bugs** — off-by-one errors, null/undefined access, wrong conditions
   - **Security** — injection, hardcoded secrets, unsafe deserialization, missing auth checks
   - **Logic errors** — incorrect state transitions, missed edge cases, broken invariants
3. Be concise. If nothing stands out, say so clearly. Skip style nitpicks and formatting opinions.
