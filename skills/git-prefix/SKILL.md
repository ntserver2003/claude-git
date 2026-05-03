---
name: git-prefix
description: Propose a commit message prefixed with a ticket ID (e.g. CMB-1234) and commit after confirmation. Auto-detects Jira-style ticket from the branch name when no ID is given. Use when the user wants a prefixed commit, mentions a ticket number, or types /git-prefix.
---

# prefix — Prefixed commit message

1. Determine the prefix:
   - If an ID argument was given (e.g. `/prefix PROJ-42`), use that.
   - Otherwise, run `git rev-parse --abbrev-ref HEAD` and extract the first `[A-Z][A-Z0-9]*-[0-9]+` match from the branch name.
   - If no prefix can be found, tell the user: "No ticket ID found in branch name. Usage: /prefix PROJ-42" and stop.
2. Run `git diff HEAD 2>/dev/null || git diff --cached`. If empty, tell the user there are no changes and stop.
3. Analyze the diff and propose a **single-line commit message** prefixed with `<ID>:` (max 72 chars total, e.g. `CMB-1234: fix null pointer in auth flow`).
4. Show the message:
   ```
     <proposed message>
   ```
5. If the user passed `-y` or `--yes`, skip to step 7.
6. Ask: `Commit with this message? [y/N]` — wait for reply. If not yes, print "Aborted." and stop.
7. Run `git add -A && git commit -m "<message>"` and report the result.
