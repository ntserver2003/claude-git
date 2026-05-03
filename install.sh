#!/usr/bin/env bash
set -euo pipefail

# claude-git installer
# curl -fsSL https://raw.githubusercontent.com/lucasnevespereira/claude-git/main/install.sh | bash

REPO="lucasnevespereira/claude-git"
INSTALL_DIR="$HOME/.local/bin"
BINARY_NAME="claude-git"

echo "Installing claude-git..."

# Check for curl
if ! command -v curl &>/dev/null; then
  echo "Error: 'curl' is required but not installed."
  exit 1
fi

# Check for claude CLI (warn only — user can use api_key mode without it)
if ! command -v claude &>/dev/null; then
  echo ""
  echo "Note: 'claude' CLI not found."
  echo "You can still use claude-git with a direct API key:"
  echo "  claude-git config api_key sk-ant-..."
  echo ""
  echo "Or install Claude Code: https://docs.anthropic.com/en/docs/claude-code"
  echo ""
fi

# Create install dir
mkdir -p "$INSTALL_DIR"

# Download
curl -fsSL "https://raw.githubusercontent.com/${REPO}/main/bin/claude-git" -o "$INSTALL_DIR/$BINARY_NAME"
chmod +x "$INSTALL_DIR/$BINARY_NAME"

# Determine shell RC file
SHELL_RC="$HOME/.zshrc"
[[ "$SHELL" == */bash ]] && SHELL_RC="$HOME/.bashrc"

# Ensure install dir is in PATH
if [[ ":$PATH:" != *":$INSTALL_DIR:"* ]]; then
  if ! grep -q '.local/bin' "$SHELL_RC" 2>/dev/null; then
    echo '' >> "$SHELL_RC"
    echo 'export PATH="$HOME/.local/bin:$PATH"' >> "$SHELL_RC"
    echo "Added $INSTALL_DIR to PATH in $SHELL_RC"
  fi
fi

# Add aliases with block markers (idempotent)
if ! grep -q '# >>> claude-git >>>' "$SHELL_RC" 2>/dev/null; then
  {
    echo ''
    echo '# >>> claude-git >>>'
    echo 'source <(claude-git aliases)'
    echo '# <<< claude-git <<<'
  } >> "$SHELL_RC"
  echo "Added aliases to $SHELL_RC"
fi

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
