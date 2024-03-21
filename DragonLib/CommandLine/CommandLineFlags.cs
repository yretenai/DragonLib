using System.Collections.ObjectModel;

namespace DragonLib.CommandLine;

public record CommandLineFlags {
	[Flag("positionals", Positional = 0, Hidden = true)]
	public Collection<string> Positionals { get; set; } = new();

	[Flag("h", Help = "Print this help text", Aliases = new[] { "help", "?" })]
	public bool Help { get; set; }
}
