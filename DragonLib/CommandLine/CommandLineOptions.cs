namespace DragonLib.CommandLine;

public record CommandLineOptions {
    public static CommandLineOptions Empty { get; } = new() { UseHelp = true };

    public bool UseHelp { get; init; } = true;
    public string Command { get; init; } = string.Empty;
    public int SkipPositionals { get; init; }
}
