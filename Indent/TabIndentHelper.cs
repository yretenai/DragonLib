namespace DragonLib.Indent
{
    public class TabIndentHelper : IndentHelperBase
    {
        protected override string TabCharacter => "\t";

        protected override IndentHelperBase Clone() =>
            new TabIndentHelper
            {
                TabSize = TabSize,
            };
    }
}
