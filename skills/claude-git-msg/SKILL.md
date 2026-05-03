---
name: claude-git-msg
description: Propose a conventional commit message for current staged/unstaged changes without committing. Use when the user wants a commit message suggestion, asks "what should I name this commit", or types /claude-git-msg.
---

# msg — Propose a commit message

1. Run `git diff HEAD 2>/dev/null || git diff --cached` to get the diff. If empty, tell the user there are no changes.
2. Read the diff and propose a **single-line conventional commit message** (max 72 chars, e.g. `feat: add retry logic for API calls`).
3. Print the message only — do not commit.
