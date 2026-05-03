# claude-git Windows installer
# Run: irm https://raw.githubusercontent.com/ntserver2003/claude-git/main/install.ps1 | iex

$ErrorActionPreference = "Stop"

$Repo       = "ntserver2003/claude-git"
$Asset      = "claude-git-win-x64.exe"
$InstallDir = Join-Path $env:LOCALAPPDATA "Programs\claude-git"
$BinaryPath = Join-Path $InstallDir "claude-git.exe"

Write-Host "Installing claude-git..."

# ── find latest release ────────────────────────────────────────────────────

$Release    = Invoke-RestMethod "https://api.github.com/repos/$Repo/releases/latest"
$AssetUrl   = ($Release.assets | Where-Object { $_.name -eq $Asset }).browser_download_url

if (-not $AssetUrl) {
    Write-Error "Could not find release asset '$Asset'."
    exit 1
}

# ── download ───────────────────────────────────────────────────────────────

New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null

Write-Host "Downloading $Asset..."
Invoke-WebRequest -Uri $AssetUrl -OutFile $BinaryPath

# ── PATH ───────────────────────────────────────────────────────────────────

$UserPath = [Environment]::GetEnvironmentVariable("Path", "User")
if ($UserPath -notlike "*$InstallDir*") {
    [Environment]::SetEnvironmentVariable("Path", "$UserPath;$InstallDir", "User")
    Write-Host "Added $InstallDir to your user PATH."
}

# ── done ───────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "Installed to $BinaryPath"
Write-Host "Restart your terminal, then run: claude-git help"
Write-Host ""
Write-Host "To set up PowerShell aliases, add this to your `$PROFILE:"
Write-Host "  claude-git aliases | Out-String | Invoke-Expression"
