---
name: git-commit
description: Propose a conventional commit message and commit staged/unstaged changes after confirmation. Use when the user wants to commit changes, asks Claude to commit, or types /git-commit. Accepts optional -y/--yes flag to skip confirmation.
---

# commit — AI commit with confirmation

1. Run `git diff HEAD 2>/dev/null || git diff --cached` to collect the diff. If empty, tell the user there are no changes and stop.
2. Analyze the diff and produce a **single-line conventional commit message** (max 72 chars).
3. Show the message to the user:
   ```
     <proposed message>
   ```
4. If the user passed `-y` or `--yes` (or said "yes" / confirmed in the same message), skip to step 6.
5. Ask: `Commit with this message? [y/N]` — wait for the user's reply. If they say no or anything other than yes, print "Aborted." and stop.
6. Run `git add -A && git commit -m "<message>"` and report the result.
