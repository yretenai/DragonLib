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

        public object? Print(object? instance, Dictionary<object, int> visited, IndentHelperBase indent, string? name, HealingMLSettings settings)
            => instance?.ToString();
    }
}
