namespace DragonLib.CommandLine;

public record CommandLineOptions {
	public static CommandLineOptions Default { get; } = new();

	public bool UseHelp { get; init; } = true;
	public bool UseVersion { get; init; } = true;
	public string Command { get; init; } = string.Empty;
	public int SkipPositionals { get; init; }
	public PrintHelpDelegate HelpDelegate { get; init; } = CommandLineFlagsParser.PrintHelp;
	public PrintVersionDelegate VersionDelegate { get; init; } = CommandLineFlagsParser.PrintVersion;
}
