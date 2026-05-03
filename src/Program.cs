using ClaudeGit;

const string Version = "0.5.0";
const string Repo    = "ntserver2003/claude-git";

var cmd  = args.Length > 0 ? args[0] : "help";
var rest = args.Length > 1 ? args[1..] : [];

// Commands that require a git repository
if (cmd is "msg" or "commit" or "prefix" or "review" or "pr" or "explain")
{
    if (!Git.IsRepo()) Die("not a git repository.");
}

var cfg = Config.Load();

try
{
    switch (cmd)
    {
        case "msg":                  await Commands.Msg(cfg); break;
        case "commit":               await Commands.Commit(cfg, rest); break;
        case "prefix":               await Commands.Prefix(cfg, rest); break;
        case "review":               await Commands.Review(cfg); break;
        case "pr":                   await Commands.Pr(cfg, rest); break;
        case "explain":              await Commands.Explain(cfg); break;
        case "config":               Commands.HandleConfig(cfg, rest); break;
        case "aliases":              Commands.Aliases(); break;
        case "upgrade":              await Commands.Upgrade(Version, Repo); break;
        case "uninstall":            Commands.Uninstall(); break;
        case "help" or "-h" or "--help":           Commands.Help(Version); break;
        case "version" or "-v" or "--version":     Commands.PrintVersion(Version); break;
        default:
            Console.Error.WriteLine($"Unknown command: {cmd}");
            Console.Error.WriteLine("Run 'claude-git help' for usage.");
            Environment.Exit(1);
            break;
    }
}
catch (ClaudeGitException ex)
{
    Die(ex.Message);
}

static void Die(string message)
{
    Console.Error.WriteLine($"claude-git: {message}");
    Environment.Exit(1);
}
