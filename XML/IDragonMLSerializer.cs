using System.Collections.Generic;
using DragonLib.Indent;
using JetBrains.Annotations;

namespace DragonLib.XML
{
    [PublicAPI]
    public interface IDragonMLSerializer
    {
        DragonMLType OverrideTarget { get; }

        object? Print(object? instance, Dictionary<object, int> visited, IndentHelperBase indents, string? valueName, DragonMLSettings settings);
    }
}
