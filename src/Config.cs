namespace ClaudeGit;

internal sealed class Config
{
    public string Model { get; set; } = "haiku";
    public int MaxLines { get; set; } = 2000;
    public string ApiKey { get; set; } = "";
    public string Mode { get; set; } = "auto"; // auto | api | cli

    public static readonly string FilePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude-git");

    // Priority: defaults → env vars → config file (file wins, matching bash `source` behavior)
    public static Config Load()
    {
        var cfg = new Config();

        ApplyEnv(cfg);

        if (File.Exists(FilePath))
        {
            foreach (var raw in File.ReadAllLines(FilePath))
            {
                var line = raw.Trim();
                if (line.StartsWith('#') || !line.Contains('=')) continue;
                var eq = line.IndexOf('=');
                var key = line[..eq].Trim();
                var val = line[(eq + 1)..].Trim().Trim('"').Trim('\'');
                switch (key)
                {
                    case "CLAUDE_GIT_MODEL":    cfg.Model   = val; break;
                    case "CLAUDE_GIT_MAX_LINES": if (int.TryParse(val, out var n)) cfg.MaxLines = n; break;
                    case "ANTHROPIC_API_KEY":   cfg.ApiKey  = val; break;
                    case "CLAUDE_GIT_MODE":     cfg.Mode    = val; break;
                }
            }
        }

        return cfg;
    }

    public void Set(string key, string value)
    {
        switch (key)
        {
            case "model":     Model   = value; break;
            case "max_lines": MaxLines = int.Parse(value); break;
            case "api_key":   ApiKey  = value; break;
            case "mode":      Mode    = value; break;
            default: throw new ClaudeGitException($"unknown config key: {key}");
        }

        var varName = key switch
        {
            "model"     => "CLAUDE_GIT_MODEL",
            "max_lines" => "CLAUDE_GIT_MAX_LINES",
            "api_key"   => "ANTHROPIC_API_KEY",
            "mode"      => "CLAUDE_GIT_MODE",
            _ => throw new ClaudeGitException($"unknown config key: {key}")
        };

        var lines = File.Exists(FilePath)
            ? new List<string>(File.ReadAllLines(FilePath))
            : [];

        var found = false;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].StartsWith(varName + "=") || lines[i].StartsWith(varName + " ="))
            {
                lines[i] = $"{varName}=\"{value}\"";
                found = true;
                break;
            }
        }
        if (!found) lines.Add($"{varName}=\"{value}\"");

        File.WriteAllLines(FilePath, lines);
    }

    // Full model ID for direct API calls; CLI accepts shorthand as-is
    public string ResolveModelForApi() => Model switch
    {
        "haiku"  => "claude-haiku-4-5-20251001",
        "sonnet" => "claude-sonnet-4-6",
        "opus"   => "claude-opus-4-6",
        _ => Model
    };

    private static void ApplyEnv(Config cfg)
    {
        var model   = Environment.GetEnvironmentVariable("CLAUDE_GIT_MODEL");
        var maxLines = Environment.GetEnvironmentVariable("CLAUDE_GIT_MAX_LINES");
        var apiKey  = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        var mode    = Environment.GetEnvironmentVariable("CLAUDE_GIT_MODE");

        if (model != null) cfg.Model = model;
        if (maxLines != null && int.TryParse(maxLines, out var n)) cfg.MaxLines = n;
        if (apiKey != null) cfg.ApiKey = apiKey;
        if (mode != null) cfg.Mode = mode;
    }
}
