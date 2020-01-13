using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.CLI
{
    [PublicAPI]
    public abstract class ICLIFlags
    {
        [CLIFlag("", Positional = 0, Hidden = true)]
        public List<string> Positionals { get; set; } = new List<string>();

        [CLIFlag("h", Help = "Print this help text", Category = "General Options", Aliases = new[] { "help", "?" })]
        public bool Help { get; set; }
    }
}
