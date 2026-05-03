---
name: claude-git-pr
description: Generate a pull request description from commits ahead of a base branch. Use when the user wants to write a PR, asks for a PR description, or types /claude-git-pr. Accepts an optional base branch argument (default: main).
---

# pr — Generate PR description

1. Determine the base branch: use the argument if given (e.g. `/pr develop`), otherwise default to `main`.
2. Verify the base branch exists: `git rev-parse --verify <base>`. If not found, tell the user and stop.
3. Collect context:
   ```bash
   git log <base>..HEAD --oneline
   git diff <base>...HEAD
   ```
   If there are no commits ahead of the base, tell the user and stop.
4. Write a PR description in this exact format:

   ```
   ## What
   <one paragraph summary of what the PR does and why>

   ## Changes
   - <key change 1>
   - <key change 2>
   ...
   ```

   Output only the description — no title, no extra commentary.
