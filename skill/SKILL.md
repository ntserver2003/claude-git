---
name: claude-git
description: Guide for using claude-git — an AI-powered CLI that generates commit messages, code reviews, and PR descriptions using Claude. Use when the user asks about git workflows, commit messages, PR descriptions, code review automation, or when working in a repo that has claude-git installed. Covers all commands, configuration, aliases, and troubleshooting.
license: MIT
---

# claude-git

`claude-git` is a bash CLI that uses Claude (via the Anthropic API or Claude Code CLI) to automate git tasks: commit message generation, code review, PR descriptions, and change explanations.

## Commands

| Command | What it does |
|---|---|
| `claude-git msg` | Print a proposed commit message (does not commit) |
| `claude-git commit` | Propose a message, confirm, then `git add -A && git commit` |
| `claude-git commit -y` | Same, skip confirmation |
| `claude-git prefix` | Commit with a ticket prefix auto-detected from the branch name (e.g. `CMB-1234` from branch `CMB-1234-fix-login`) |
| `claude-git prefix PROJ-42` | Commit with an explicit prefix |
| `claude-git prefix PROJ-42 -y` | Same, skip confirmation |
| `claude-git review` | Review staged/unstaged changes for bugs, security issues, logic errors |
| `claude-git pr` | Generate a PR description from commits ahead of `main` |
| `claude-git pr develop` | PR description against a different base branch |
| `claude-git explain` | Plain-English explanation of current changes (2–3 sentences) |
| `claude-git config` | Show all config values |
| `claude-git config <key>` | Show one config value |
| `claude-git config <key> <val>` | Set a config value |
| `claude-git aliases` | Print shell aliases to add |
| `claude-git upgrade` | Self-update to the latest GitHub release |
| `claude-git uninstall` | Remove binary and config |

## Shell Aliases

| Alias | Command |
|---|---|
| `cg` | `claude-git` |
| `cgm` | `claude-git msg` |
| `cgc` | `claude-git commit` |
| `cgcy` | `claude-git commit --yes` |
| `cgrev` | `claude-git review` |
| `cgpr` | `claude-git pr` |
| `cgex` | `claude-git explain` |
| `cgpx` | `claude-git prefix` |

## Configuration

Config file: `~/.claude-git` (sourced as bash variables on startup).

| Key | Default | Description |
|---|---|---|
| `model` | `haiku` | Claude model: `haiku`, `sonnet`, `opus`, or a full model ID |
| `max_lines` | `2000` | Max diff lines sent to Claude (truncates silently with a warning) |
| `api_key` | _(not set)_ | Anthropic API key — enables direct API calls (faster than CLI) |
| `mode` | `auto` | `auto` (API if key set, else CLI), `api` (API only), `cli` (CLI only) |

```bash
claude-git config model sonnet       # switch model
claude-git config max_lines 5000     # larger diffs
claude-git config api_key sk-ant-... # enable API mode
claude-git config mode api           # force API only
```

## How It Works

**Mode resolution (`auto`):** If `ANTHROPIC_API_KEY` is set, the tool tries the API first. On failure it falls back to Claude Code CLI (`claude -p`). If no key, it goes straight to CLI.

**Diff collection:** Uses `git diff HEAD` (or `git diff --cached` for initial commits). Truncates to `max_lines` with a warning.

**Prefix detection:** `prefix` without an argument greps the current branch name for a Jira-style ticket ID (`[A-Z][A-Z0-9]*-[0-9]+`). Fails clearly if none found.

**PR diff:** `pr` compares `<base>...HEAD` (three-dot diff) and includes `git log <base>..HEAD --oneline` for commit context.

## Install

**macOS / Linux:**
```bash
curl -fsSL https://raw.githubusercontent.com/lucasnevespereira/claude-git/main/install.sh | bash
```
Installs to `~/.local/bin/claude-git` and appends aliases to `.zshrc` / `.bashrc`.

**Windows:**

`claude-git` is a bash script. On Windows it runs inside a bash environment:

| Environment | Notes |
|---|---|
| **WSL** (recommended) | Full Linux environment. Run the standard install inside WSL. Config lives at `~/.claude-git` within WSL. |
| **Git Bash** (MINGW) | Ships with Git for Windows. Run the curl install inside Git Bash. `~` resolves to `C:\Users\<name>`. |
| **Cygwin** | Works, but WSL is simpler. |

In Git Bash or WSL, the install command is identical:
```bash
curl -fsSL https://raw.githubusercontent.com/lucasnevespereira/claude-git/main/install.sh | bash
```

After install, ensure `~/.local/bin` is on `PATH`. In Git Bash, add to `~/.bashrc`:
```bash
export PATH="$HOME/.local/bin:$PATH"
```

**PowerShell / CMD are not supported** — they cannot execute bash scripts natively. Direct the user to WSL or Git Bash.

## Troubleshooting

- **"no changes to commit"** — no staged or unstaged tracked-file changes exist.
- **"not a git repository"** — run from inside a git repo.
- **"branch 'main' not found"** — pass the correct base branch: `claude-git pr develop`.
- **"API error: ..."** — check `ANTHROPIC_API_KEY` or switch to `mode cli`.
- **"'claude' is required but not installed"** — install Claude Code CLI, or set an API key and use `mode api`.
- **Large diffs truncated** — increase `max_lines`: `claude-git config max_lines 5000`.

**Windows-specific:**
- **"command not found: claude-git"** in Git Bash — `~/.local/bin` is not on `PATH`. Add `export PATH="$HOME/.local/bin:$PATH"` to `~/.bashrc` and `source ~/.bashrc`.
- **"command not found: claude-git"** in WSL — same fix; also verify the install ran inside WSL, not Windows PowerShell.
- **curl not found** in Git Bash — Git for Windows includes curl since v2.26; upgrade Git or install curl separately.
- **Line-ending issues (`\r` errors)** — if the script was downloaded with Windows line endings, run `sed -i 's/\r//' ~/.local/bin/claude-git` inside bash.
- **`~/.claude-git` config not found** — on Git Bash, `~` is `C:\Users\<name>`. The file is `C:\Users\<name>\.claude-git`. Both `claude-git config` commands and manual editing work from Git Bash.

## Model Shorthand Resolution

| Shorthand | Full model ID |
|---|---|
| `haiku` | `claude-haiku-4-5-20251001` |
| `sonnet` | `claude-sonnet-4-6` |
| `opus` | `claude-opus-4-6` |

Any other value is passed through as-is, so full model IDs work too.
