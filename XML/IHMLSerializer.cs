using System;
using System.Collections.Generic;
using DragonLib.Indent;
using JetBrains.Annotations;

namespace DragonLib.XML
{
    [PublicAPI]
    public interface IHMLSerializer
    {
        HMLSerializationTarget OverrideTarget { get; }

        object? Print(object? instance, IReadOnlyDictionary<Type, IHMLSerializer> customTypeSerializers, HashSet<object?> visited, IndentHelperBase indents, string? valueName);
    }
}
