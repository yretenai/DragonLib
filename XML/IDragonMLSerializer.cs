using System.Collections.Generic;
using DragonLib.Indent;

namespace DragonLib.XML {
    public interface IDragonMLSerializer {
        DragonMLType OverrideTarget { get; }

        object? Print(object? instance, Dictionary<object, int> visited, IndentHelperBase indents, string? valueName, DragonMLSettings settings);
    }
}
