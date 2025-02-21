using System.Collections.ObjectModel;

namespace DragonLib.CommandLine;

public record CommandLineFlags {
	[Flag("positionals", Positional = 0, Hidden = true)]
	public Collection<string> Positionals { get; set; } = [];

	[Flag("h", Help = "Print this help text", Aliases = ["help", "?"])]
	public bool Help { get; set; }

	[Flag("v", Help = "Print this help text", Aliases = ["version"])]
	public bool Version { get; set; }
}
