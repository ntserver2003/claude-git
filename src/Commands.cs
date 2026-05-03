using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ClaudeGit;

internal static class Commands
{
    // ── git-aware commands ─────────────────────────────────────────────────

    public static async Task Msg(Config cfg)
    {
        if (!Git.HasChanges()) throw new ClaudeGitException("no changes to commit.");
        var msg = await Claude.CallAsync(
            Git.GetDiff(cfg.MaxLines),
            "propose a conventional commit message for these changes. one line, max 72 chars. output only the message, nothing else.",
            256, cfg);
        Console.WriteLine(msg);
    }

    public static async Task Commit(Config cfg, string[] args)
    {
        var autoYes = HasFlag(args, "-y", "--yes");
        if (!Git.HasChanges()) throw new ClaudeGitException("no changes to commit.");

        var msg = await Claude.CallAsync(
            Git.GetDiff(cfg.MaxLines),
            "propose a conventional commit message for these changes. one line, max 72 chars. output only the message, nothing else.",
            256, cfg);

        if (string.IsNullOrEmpty(msg)) throw new ClaudeGitException("failed to generate a commit message.");

        Console.WriteLine($"\n  {msg}\n");

        if (autoYes || Confirm("Commit with this message?"))
        {
            Git.AddAll();
            Git.Commit(msg);
        }
        else Console.WriteLine("Aborted.");
    }

    public static async Task Prefix(Config cfg, string[] args)
    {
        var prefix  = args.Length > 0 && !args[0].StartsWith('-') ? args[0] : null;
        var autoYes = HasFlag(args, "-y", "--yes");

        if (prefix == null)
        {
            prefix = Git.GetTicketFromBranch()
                ?? throw new ClaudeGitException("no ticket ID found in branch name. Usage: claude-git prefix <ID>");
        }

        if (!Git.HasChanges()) throw new ClaudeGitException("no changes to commit.");

        var msg = await Claude.CallAsync(
            Git.GetDiff(cfg.MaxLines),
            $"propose a commit message for these changes prefixed with '{prefix}:'. one line, max 72 chars. output only the message, nothing else.",
            256, cfg);

        if (string.IsNullOrEmpty(msg)) throw new ClaudeGitException("failed to generate a commit message.");

        Console.WriteLine($"\n  {msg}\n");

        if (autoYes || Confirm("Commit with this message?"))
        {
            Git.AddAll();
            Git.Commit(msg);
        }
        else Console.WriteLine("Aborted.");
    }

    public static async Task Review(Config cfg)
    {
        if (!Git.HasChanges()) throw new ClaudeGitException("no changes to review.");
        var review = await Claude.CallAsync(
            Git.GetDiff(cfg.MaxLines),
            "review this diff. be concise. flag bugs, security issues, and logic errors. if it looks good, say so. skip style nitpicks.",
            1024, cfg);
        Console.WriteLine(review);
    }

    public static async Task Pr(Config cfg, string[] args)
    {
        var baseBranch = args.Length > 0 ? args[0] : "main";
        var pr = await Claude.CallAsync(
            Git.GetBranchDiff(baseBranch, cfg.MaxLines),
            "write a pull request description for these changes. format:\n\n## What\none paragraph summary.\n\n## Changes\n- bullet points of key changes\n\noutput only the description, no title.",
            1024, cfg);
        Console.WriteLine(pr);
    }

    public static async Task Explain(Config cfg)
    {
        if (!Git.HasChanges()) throw new ClaudeGitException("no changes to explain.");
        var explanation = await Claude.CallAsync(
            Git.GetDiff(cfg.MaxLines),
            "explain what these changes do in 2-3 sentences. be specific about the behavior change, not just what files were touched.",
            512, cfg);
        Console.WriteLine(explanation);
    }

    // ── config ─────────────────────────────────────────────────────────────

    public static void HandleConfig(Config cfg, string[] args)
    {
        if (args.Length == 0) { ShowAllConfig(cfg); return; }

        var key   = args[0];
        var value = args.Length > 1 ? args[1] : null;

        ValidateKey(key);

        if (value == null) { ShowOneConfig(cfg, key); return; }

        ValidateValue(key, value);
        cfg.Set(key, value);
        Console.WriteLine($"{key} = {value}");
    }

    // ── other ──────────────────────────────────────────────────────────────

    public static void Aliases()
    {
        if (OperatingSystem.IsWindows())
        {
            Console.WriteLine("# claude-git PowerShell aliases — add to your $PROFILE");
            Console.WriteLine("Set-Alias -Name cg   -Value claude-git");
            Console.WriteLine("function cgm   { claude-git msg @args }");
            Console.WriteLine("function cgc   { claude-git commit @args }");
            Console.WriteLine("function cgcy  { claude-git commit --yes @args }");
            Console.WriteLine("function cgrev { claude-git review @args }");
            Console.WriteLine("function cgpr  { claude-git pr @args }");
            Console.WriteLine("function cgex  { claude-git explain @args }");
            Console.WriteLine("function cgpx  { claude-git prefix @args }");
        }
        else
        {
            Console.WriteLine("# claude-git aliases");
            Console.WriteLine("alias cg='claude-git'");
            Console.WriteLine("alias cgm='claude-git msg'");
            Console.WriteLine("alias cgc='claude-git commit'");
            Console.WriteLine("alias cgcy='claude-git commit --yes'");
            Console.WriteLine("alias cgrev='claude-git review'");
            Console.WriteLine("alias cgpr='claude-git pr'");
            Console.WriteLine("alias cgex='claude-git explain'");
            Console.WriteLine("alias cgpx='claude-git prefix'");
        }
    }

    public static async Task Upgrade(string currentVersion, string repo)
    {
        Console.WriteLine("Checking for updates...");

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", "claude-git");

        var json    = await http.GetStringAsync($"https://api.github.com/repos/{repo}/releases/latest");
        var release = JsonSerializer.Deserialize(json, AppJsonContext.Default.GitHubRelease)
            ?? throw new ClaudeGitException("could not parse release information.");

        if (release.TagName == $"v{currentVersion}")
        {
            Console.WriteLine($"Already on latest version (v{currentVersion}).");
            return;
        }

        var assetName = GetAssetName();
        var asset     = Array.Find(release.Assets, a => a.Name == assetName)
            ?? throw new ClaudeGitException($"no release asset found for this platform ({assetName}).");

        var currentPath = Environment.ProcessPath
            ?? throw new ClaudeGitException("could not determine current binary path.");

        Console.WriteLine($"Updating v{currentVersion} → {release.TagName}...");

        var newPath = currentPath + ".new";
        var bytes   = await http.GetByteArrayAsync(asset.BrowserDownloadUrl);
        await File.WriteAllBytesAsync(newPath, bytes);

        if (OperatingSystem.IsWindows())
        {
            // Cannot replace a running executable on Windows; give the user a one-liner
            Console.WriteLine("Download complete. To finish upgrading, run:");
            Console.WriteLine($"  Move-Item -Force '{newPath}' '{currentPath}'");
        }
        else
        {
            File.Move(newPath, currentPath, overwrite: true);
            File.SetUnixFileMode(currentPath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
            Console.WriteLine($"Updated to {release.TagName}.");
        }
    }

    public static void Uninstall()
    {
        Console.Write("Remove claude-git and its config? [y/N] ");
        if (!string.Equals(Console.ReadLine()?.Trim(), "y", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Aborted.");
            return;
        }

        Console.WriteLine("Uninstalling claude-git...");

        var currentPath = Environment.ProcessPath;
        if (currentPath != null && File.Exists(currentPath))
        {
            if (OperatingSystem.IsWindows())
                Console.WriteLine($"Close this terminal then delete manually: {currentPath}");
            else
                File.Delete(currentPath);
        }

        if (File.Exists(Config.FilePath))
            File.Delete(Config.FilePath);

        if (!OperatingSystem.IsWindows())
            RemoveShellBlock();

        Console.WriteLine("Removed. Restart your shell.");
    }

    public static void Help(string version) => Console.WriteLine($"""
        claude-git v{version} — AI-powered git helpers

        Usage: claude-git <command> [options]

        Commands:
          msg                 Propose a commit message (print only)
          commit [--yes|-y]   Propose + commit (--yes skips confirmation)
          prefix [ID] [-y]    Commit with ticket prefix (auto-detects from branch name)
          review              Review staged/unstaged changes for bugs
          pr [base]           Generate PR description (default base: main)
          explain             Explain what the current changes do
          config              Show current config
          config <key>        Show one config value
          config <key> <val>  Set a config value
          aliases             Print shell aliases to add
          upgrade             Update to the latest version
          uninstall           Remove claude-git from your system

        Config keys:
          model       Claude model shorthand or full ID (default: haiku)
          max_lines   Max diff lines sent to Claude (default: 2000)
          api_key     Anthropic API key (enables direct API mode)
          mode        auto | api | cli  (default: auto)
        """);

    public static void PrintVersion(string version) => Console.WriteLine($"claude-git v{version}");

    // ── helpers ────────────────────────────────────────────────────────────

    private static bool Confirm(string prompt)
    {
        Console.Write($"{prompt} [y/N] ");
        return string.Equals(Console.ReadLine()?.Trim(), "y", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasFlag(string[] args, params string[] flags)
        => args.Any(a => flags.Contains(a));

    private static void ValidateKey(string key)
    {
        if (key is not ("model" or "max_lines" or "api_key" or "mode"))
            throw new ClaudeGitException($"unknown config key: {key} (available: model, max_lines, api_key, mode)");
    }

    private static void ValidateValue(string key, string value)
    {
        switch (key)
        {
            case "max_lines" when !int.TryParse(value, out _):
                throw new ClaudeGitException("max_lines must be a number.");
            case "mode" when value is not ("auto" or "api" or "cli"):
                throw new ClaudeGitException("mode must be: auto, api, or cli.");
        }
    }

    private static void ShowAllConfig(Config cfg)
    {
        Console.WriteLine($"model     = {cfg.Model}");
        Console.WriteLine($"max_lines = {cfg.MaxLines}");
        Console.WriteLine($"api_key   = {(string.IsNullOrEmpty(cfg.ApiKey) ? "(not set)" : "(set)")}");
        Console.WriteLine($"mode      = {cfg.Mode}");
        Console.WriteLine();
        Console.WriteLine(File.Exists(Config.FilePath)
            ? $"config: {Config.FilePath}"
            : "config: (defaults, no config file)");
    }

    private static void ShowOneConfig(Config cfg, string key) => Console.WriteLine(key switch
    {
        "model"     => cfg.Model,
        "max_lines" => cfg.MaxLines.ToString(),
        "api_key"   => string.IsNullOrEmpty(cfg.ApiKey) ? "(not set)" : "(set)",
        "mode"      => cfg.Mode,
        _ => throw new ClaudeGitException($"unknown config key: {key}")
    });

    private static string GetAssetName()
    {
        if (OperatingSystem.IsWindows()) return "claude-git-win-x64.exe";
        if (OperatingSystem.IsMacOS())
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? "claude-git-osx-arm64"
                : "claude-git-osx-x64";
        return "claude-git-linux-x64";
    }

    private static void RemoveShellBlock()
    {
        var shell  = Environment.GetEnvironmentVariable("SHELL") ?? "";
        var rcFile = shell.EndsWith("bash")
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".bashrc")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zshrc");

        if (!File.Exists(rcFile)) return;

        var lines = new List<string>(File.ReadAllLines(rcFile));
        var start = lines.FindIndex(l => l.Contains("# >>> claude-git >>>"));
        var end   = lines.FindIndex(l => l.Contains("# <<< claude-git <<<"));
        if (start >= 0 && end > start)
            lines.RemoveRange(start, end - start + 1);

        File.WriteAllLines(rcFile, lines);
    }
}
