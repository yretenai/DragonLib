using System;
using System.Collections.Generic;
using DragonLib.Indent;
using JetBrains.Annotations;

namespace DragonLib.XML
{
    [PublicAPI]
    public class HMLToStringSerializer : IHMLSerializer
    {
        public static readonly IHMLSerializer Default = new HMLToStringSerializer();

        public HMLSerializationTarget OverrideTarget => HMLSerializationTarget.Value;

        public object? Print(object? instance, IReadOnlyDictionary<Type, IHMLSerializer> custom, HashSet<object?> visited, IndentHelperBase indent, string? name) => instance?.ToString();
    }
}
