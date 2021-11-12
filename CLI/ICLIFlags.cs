using System.Collections.Generic;

namespace DragonLib.CLI {
    public record ICLIFlags {
        [CLIFlag("positionals", Positional = 0, Hidden = true)]
        public List<string> Positionals { get; set; } = new();

        [CLIFlag("h", Help = "Print this help text", Category = "General Options", Aliases = new[] { "help", "?" })]
        public bool Help { get; set; }
    }
}
