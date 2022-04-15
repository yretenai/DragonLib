namespace DragonLib.CommandLine;

public record CommandLineFlags {
    [Flag("positionals", Positional = 0, Hidden = true)]
    public List<string> Positionals { get; set; } = new();

    [Flag("h", Help = "Print this help text", Category = "General Options", Aliases = new[] { "help", "?" })]
    public bool Help { get; set; }
}
