using System.Collections.Generic;
using DragonLib.Indent;
using JetBrains.Annotations;

namespace DragonLib.XML
{
    [PublicAPI]
    public interface IHMLSerializer
    {
        HMLSerializationTarget OverrideTarget { get; }

        object? Print(object? instance, Dictionary<object, int> visited, IndentHelperBase indents, string? valueName, HealingMLSettings settings);
    }
}
