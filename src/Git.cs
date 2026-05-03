using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ClaudeGit;

internal static partial class Git
{
    [GeneratedRegex(@"([A-Z][A-Z0-9]*-[0-9]+)")]
    private static partial Regex TicketPattern();

    public static bool IsRepo()
        => Exec("rev-parse", "--git-dir").ExitCode == 0;

    public static bool HasChanges()
    {
        var diff   = Exec("diff", "--quiet");
        var cached = Exec("diff", "--cached", "--quiet");
        return diff.ExitCode != 0 || cached.ExitCode != 0;
    }

    public static string GetDiff(int maxLines)
    {
        var result = Exec("diff", "HEAD");
        var diff = result.Output;

        if (string.IsNullOrEmpty(diff))
            diff = Exec("diff", "--cached").Output;

        if (string.IsNullOrEmpty(diff))
            throw new ClaudeGitException("no diff found. Stage changes or modify tracked files.");

        return Truncate(diff, maxLines, "diff truncated");
    }

    public static string GetBranchDiff(string baseBranch, int maxLines)
    {
        if (Exec("rev-parse", "--verify", baseBranch).ExitCode != 0)
            throw new ClaudeGitException($"branch '{baseBranch}' not found.");

        var commits = Exec("log", $"{baseBranch}..HEAD", "--oneline").Output.Trim();
        if (string.IsNullOrEmpty(commits))
            throw new ClaudeGitException($"no commits ahead of '{baseBranch}'.");

        var diff = Truncate(Exec("diff", $"{baseBranch}...HEAD").Output, maxLines, "diff truncated");
        return $"=== Commits ===\n{commits}\n\n=== Diff ===\n{diff}";
    }

    public static string? GetTicketFromBranch()
    {
        var branch = Exec("rev-parse", "--abbrev-ref", "HEAD").Output.Trim();
        var m = TicketPattern().Match(branch);
        return m.Success ? m.Groups[1].Value : null;
    }

    public static void AddAll()
        => ExecOrThrow("add -A failed", "add", "-A");

    public static void Commit(string message)
        => ExecOrThrow("commit failed", "commit", "-m", message);

    // ── internals ──────────────────────────────────────────────────────────

    private static string Truncate(string text, int maxLines, string label)
    {
        var lines = text.Split('\n');
        if (lines.Length <= maxLines) return text;
        Console.Error.WriteLine(
            $"claude-git: ({label}: {lines.Length} → {maxLines} lines. adjust with: claude-git config max_lines <n>)");
        return string.Join('\n', lines[..maxLines]);
    }

    private static (int ExitCode, string Output) Exec(params string[] args)
    {
        using var proc = new Process();
        proc.StartInfo = BuildPsi(args);
        proc.Start();
        var output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();
        return (proc.ExitCode, output);
    }

    private static void ExecOrThrow(string errorMsg, params string[] args)
    {
        if (Exec(args).ExitCode != 0)
            throw new ClaudeGitException(errorMsg);
    }

    private static ProcessStartInfo BuildPsi(string[] args)
    {
        var psi = new ProcessStartInfo("git")
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
        };
        foreach (var a in args) psi.ArgumentList.Add(a);
        return psi;
    }
}
