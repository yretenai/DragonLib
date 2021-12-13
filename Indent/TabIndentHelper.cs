namespace DragonLib.Indent {
    public class TabIndentHelper : IndentHelperBase {
        protected override string TabCharacter => "\t";

        protected override IndentHelperBase Clone() {
            return new TabIndentHelper {
                TabSize = TabSize
            };
        }
    }
}
