using DragonLib.Indent;

namespace DragonLib.Xml;

public interface IDragonMarkupSerializer {
    DragonMarkupType OverrideTarget { get; }

    object? Print(object? instance,
        Dictionary<object, int> visited,
        IndentHelperBase indents,
        string? valueName,
        DragonMarkupSettings settings);
}
