using JetBrains.Annotations;

namespace DragonLib.Indent
{
    [PublicAPI]
    public class TabIndentHelper : IndentHelperBase
    {
        protected override string TabCharacter { get; } = "\t";

        protected override IndentHelperBase Clone() =>
            new TabIndentHelper
            {
                TabSize = TabSize
            };
    }
}
