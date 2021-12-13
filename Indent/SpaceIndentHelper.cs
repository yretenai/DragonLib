namespace DragonLib.Indent {
    public class SpaceIndentHelper : IndentHelperBase {
        protected override string TabCharacter => "  ";

        protected override IndentHelperBase Clone() {
            return new SpaceIndentHelper {
                TabSize = TabSize
            };
        }
    }
}
