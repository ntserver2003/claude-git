using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ClaudeGit;

internal static class Claude
{
    private static readonly HttpClient Http = new();

    static Claude()
    {
        Http.DefaultRequestHeaders.Add("User-Agent", "claude-git");
    }

    public static async Task<string> CallAsync(string input, string prompt, int maxTokens, Config cfg)
    {
        if (string.IsNullOrEmpty(input))
            throw new ClaudeGitException("no input to send to Claude.");

        return cfg.Mode switch
        {
            "api" => await CallApiAsync(input, prompt, maxTokens, cfg),
            "cli" => await CallCliAsync(input, prompt, cfg),
            _     => await CallAutoAsync(input, prompt, maxTokens, cfg),
        };
    }

    private static async Task<string> CallAutoAsync(string input, string prompt, int maxTokens, Config cfg)
    {
        if (!string.IsNullOrEmpty(cfg.ApiKey))
        {
            try { return await CallApiAsync(input, prompt, maxTokens, cfg); }
            catch (ClaudeGitException) { throw; }  // re-throw fatal errors (bad key, etc.)
            catch { Console.Error.WriteLine("claude-git: API unavailable, using CLI..."); }
        }
        return await CallCliAsync(input, prompt, cfg);
    }

    private static async Task<string> CallApiAsync(string input, string prompt, int maxTokens, Config cfg)
    {
        if (string.IsNullOrEmpty(cfg.ApiKey))
            throw new ClaudeGitException("mode is 'api' but no API key configured. Run: claude-git config api_key <key>");

        var request = new ApiRequest(
            cfg.ResolveModelForApi(),
            maxTokens,
            [new ApiMessage("user", $"{input}\n\n{prompt}")]
        );

        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        req.Headers.Add("x-api-key", cfg.ApiKey);
        req.Headers.Add("anthropic-version", "2023-06-01");
        req.Content = new StringContent(
            JsonSerializer.Serialize(request, AppJsonContext.Default.ApiRequest),
            Encoding.UTF8, "application/json");

        using var resp = await Http.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        var parsed = JsonSerializer.Deserialize(body, AppJsonContext.Default.ApiResponse);

        if (parsed?.Error != null)
            throw new ClaudeGitException($"API error: {parsed.Error.Message}");

        var text = parsed?.Content?.FirstOrDefault(c => c.Type == "text")?.Text;
        if (string.IsNullOrEmpty(text))
            throw new ClaudeGitException("API returned an empty response.");

        return text.Trim();
    }

    private static async Task<string> CallCliAsync(string input, string prompt, Config cfg)
    {
        if (!IsAvailable("claude"))
            throw new ClaudeGitException(
                "'claude' is required but not installed. " +
                "Install Claude Code: https://docs.anthropic.com/en/docs/claude-code  " +
                "Or set an API key: claude-git config api_key <key>");

        using var proc = new Process();
        var psi = new ProcessStartInfo("claude")
        {
            RedirectStandardInput  = true,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
        };
        psi.ArgumentList.Add("-p");
        psi.ArgumentList.Add(prompt);
        psi.ArgumentList.Add("--model");
        psi.ArgumentList.Add(cfg.Model);
        psi.ArgumentList.Add("--output-format");
        psi.ArgumentList.Add("text");
        psi.ArgumentList.Add("--no-session-persistence");
        psi.ArgumentList.Add("--effort");
        psi.ArgumentList.Add("low");
        proc.StartInfo = psi;
        proc.Start();

        await proc.StandardInput.WriteAsync(input);
        proc.StandardInput.Close();

        var output = await proc.StandardOutput.ReadToEndAsync();
        await proc.WaitForExitAsync();

        if (proc.ExitCode != 0)
            throw new ClaudeGitException("Claude CLI returned a non-zero exit code.");

        return output.Trim();
    }

    private static bool IsAvailable(string command)
    {
        var probe = OperatingSystem.IsWindows() ? "where" : "which";
        try
        {
            using var proc = new Process();
            proc.StartInfo = new ProcessStartInfo(probe, command)
            {
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
            };
            proc.Start();
            proc.WaitForExit();
            return proc.ExitCode == 0;
        }
        catch { return false; }
    }
}
