using JetBrains.Annotations;

namespace DragonLib.Indent
{
    [PublicAPI]
    public class SpaceIndentHelper : IndentHelperBase
    {
        protected override string TabCharacter { get; } = "  ";

        protected override IndentHelperBase Clone() =>
            new SpaceIndentHelper
            {
                TabSize = TabSize
            };
    }
}
