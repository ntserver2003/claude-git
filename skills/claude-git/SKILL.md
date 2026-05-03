---
name: claude-git
description: AI-powered git helpers — commit messages, code review, PR descriptions, and change explanations. Use when the user asks about git workflows, wants to commit/review/describe changes, or is working with claude-git. This is the index skill; individual commands are in the sibling skill files.
license: MIT
---

# claude-git skills

This package ships six Claude Code skills that replicate the `claude-git` bash CLI using Claude's native analysis. No API key or external binary required.

| Skill | Slash command | What it does |
|---|---|---|
| [msg.md](msg.md) | `/claude-git-msg` | Print a proposed commit message (no commit) |
| [commit.md](commit.md) | `/claude-git-commit [-y]` | Propose + commit with optional confirmation skip |
| [prefix.md](prefix.md) | `/claude-git-prefix [ID] [-y]` | Commit with ticket prefix (auto-detects from branch) |
| [review.md](review.md) | `/claude-git-review` | Review changes for bugs, security issues, logic errors |
| [pr.md](pr.md) | `/claude-git-pr [base]` | Generate PR description (default base: `main`) |
| [explain.md](explain.md) | `/claude-git-explain` | Plain-English explanation of current changes |

## Quick reference

```
/claude-git-msg                    → print proposed commit message
/claude-git-commit                 → propose + confirm + commit
/claude-git-commit -y              → propose + commit (no confirmation)
/claude-git-prefix                 → commit with ticket from branch name
/claude-git-prefix PROJ-42         → commit with explicit ticket prefix
/claude-git-prefix PROJ-42 -y      → same, skip confirmation
/claude-git-review                 → diff review: bugs, security, logic
/claude-git-pr                     → PR description vs main
/claude-git-pr develop             → PR description vs develop
/claude-git-explain                → what do my current changes do?
```

## How it works

Each skill:
1. Runs `git diff HEAD` (falls back to `--cached` for initial commits) via the Bash tool.
2. Analyzes the diff directly — no external API call, no `claude` binary needed.
3. Produces output or runs git commands based on the command's contract.

## Differences from the bash CLI

| bash `claude-git` | skills equivalent |
|---|---|
| Requires install + PATH setup | Works anywhere Claude Code is running |
| Calls Anthropic API or `claude` CLI | Claude analyzes inline — no extra config |
| `mode`, `api_key`, `model` config | Model is the active Claude session |
| `upgrade` / `uninstall` commands | Not needed — skills are files in the repo |
