using System.Collections.Generic;
using DragonLib.Indent;
using JetBrains.Annotations;

namespace DragonLib.XML
{
    [PublicAPI]
    public class DragonMLToStringSerializer : IDragonMLSerializer
    {
        public static readonly IDragonMLSerializer Default = new DragonMLToStringSerializer();

        public DragonMLType OverrideTarget => DragonMLType.Value;

        public object? Print(object? instance, Dictionary<object, int> visited, IndentHelperBase indent, string? name, DragonMLSettings settings)
            => instance?.ToString();
    }
}
