#!/usr/bin/env bash
set -euo pipefail

# claude-git installer
# curl -fsSL https://raw.githubusercontent.com/ntserver2003/claude-git/main/install.sh | bash

REPO="ntserver2003/claude-git"
INSTALL_DIR="$HOME/.local/bin"
BINARY_NAME="claude-git"

echo "Installing claude-git..."

# ── prerequisites ──────────────────────────────────────────────────────────

if ! command -v curl &>/dev/null; then
  echo "Error: 'curl' is required but not installed." >&2
  exit 1
fi

# ── detect platform ────────────────────────────────────────────────────────

OS=$(uname -s)
ARCH=$(uname -m)

case "$OS" in
  Darwin)
    case "$ARCH" in
      arm64) ASSET="claude-git-osx-arm64" ;;
      *)
        echo "Error: Unsupported macOS architecture: $ARCH" >&2
        echo "Only Apple Silicon (arm64 / M-series) is supported." >&2
        exit 1
        ;;
    esac
    ;;
  Linux)
    case "$ARCH" in
      x86_64) ASSET="claude-git-linux-x64" ;;
      *)
        echo "Error: Unsupported Linux architecture: $ARCH" >&2
        echo "Only x86_64 is supported." >&2
        exit 1
        ;;
    esac
    ;;
  *)
    echo "Error: Unsupported OS: $OS" >&2
    echo "On Windows use: irm https://raw.githubusercontent.com/ntserver2003/claude-git/main/install.ps1 | iex" >&2
    exit 1
    ;;
esac

# ── find latest release ────────────────────────────────────────────────────

echo "Fetching latest release..."
DOWNLOAD_URL=$(curl -fsSL "https://api.github.com/repos/${REPO}/releases/latest" \
  | grep "browser_download_url" \
  | grep "\"${ASSET}\"" \
  | cut -d'"' -f4)

if [[ -z "$DOWNLOAD_URL" ]]; then
  echo "Error: Could not find a release asset for $ASSET" >&2
  exit 1
fi

# ── install ────────────────────────────────────────────────────────────────

mkdir -p "$INSTALL_DIR"

echo "Downloading $ASSET..."
curl -fsSL "$DOWNLOAD_URL" -o "$INSTALL_DIR/$BINARY_NAME"
chmod +x "$INSTALL_DIR/$BINARY_NAME"

# ── PATH ───────────────────────────────────────────────────────────────────

SHELL_RC="$HOME/.zshrc"
[[ "$SHELL" == */bash ]] && SHELL_RC="$HOME/.bashrc"

if [[ ":$PATH:" != *":$INSTALL_DIR:"* ]]; then
  if ! grep -q '.local/bin' "$SHELL_RC" 2>/dev/null; then
    echo '' >> "$SHELL_RC"
    echo 'export PATH="$HOME/.local/bin:$PATH"' >> "$SHELL_RC"
    echo "Added $INSTALL_DIR to PATH in $SHELL_RC"
  fi
fi

# ── aliases (idempotent) ───────────────────────────────────────────────────

if ! grep -q '# >>> claude-git >>>' "$SHELL_RC" 2>/dev/null; then
  {
    echo ''
    echo '# >>> claude-git >>>'
    echo 'source <(claude-git aliases)'
    echo '# <<< claude-git <<<'
  } >> "$SHELL_RC"
  echo "Added aliases to $SHELL_RC"
fi

# ── done ───────────────────────────────────────────────────────────────────

echo ""
echo "Installed! Run: source $SHELL_RC"
echo ""
echo "Commands:"
echo "  claude-git msg       Propose a commit message"
echo "  claude-git commit    Propose + commit"
echo "  claude-git review    Review changes for bugs"
echo "  claude-git pr        Generate PR description"
echo "  claude-git explain   Explain current changes"
echo ""
echo "Aliases: cg, cgm, cgc, cgcy, cgrev, cgpr, cgex, cgpx"
