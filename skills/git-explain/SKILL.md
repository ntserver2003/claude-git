---
name: git-explain
description: Explain what the current staged/unstaged changes do in plain English. Use when the user asks "what does this change do", "explain my changes", or types /git-explain.
---

# explain — Explain current changes

1. Run `git diff HEAD 2>/dev/null || git diff --cached`. If empty, tell the user there are no changes.
2. Write a **2–3 sentence** plain-English explanation of what the changes do. Be specific about the behavior change — not just which files were touched. Avoid implementation details unless they clarify the impact.
