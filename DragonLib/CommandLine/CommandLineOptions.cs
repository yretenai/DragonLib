namespace DragonLib.CommandLine;

public record struct CommandLineOptions {
    public static CommandLineOptions Empty => new();

    public bool UseHelp { get; init; }
}
