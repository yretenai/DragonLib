using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace DragonLib
{
    [PublicAPI]
    public static class DataTypeHelper
    {
        public static Dictionary<Type, Dictionary<string, string>> Cache { get; } = new Dictionary<Type, Dictionary<string, string>>();

        public static void Preload<T>() where T : struct
        {
            var type = typeof(T);
            var values = Enum.GetValues(type);
            Cache[type] = new Dictionary<string, string>();
            for (var i = 0; i < values.Length; ++i)
            {
                var value = values.GetValue(i)?.ToString();
                if (value == null) continue;
                var extension = value.ToLower();
                var custom = type.GetMember(value).SelectMany(x => x.GetCustomAttributes(false)).ToArray();
                if (custom.Any(x => x is FileExtensionAttribute)) extension = ((FileExtensionAttribute) custom.First(x => x is FileExtensionAttribute)).Extension;
                Cache[type][value] = extension;
            }
        }

        public static string GetExtension<T>(T magic) where T : struct
        {
            var type = typeof(T);
            if (!Cache.TryGetValue(type, out var cache)) return "bin";

            return cache.TryGetValue(magic.ToString() ?? string.Empty, out var ext) ? ext : "bin";
        }

        public static bool IsKnown<T>(T magic) where T : struct => Cache.TryGetValue(typeof(T), out var cache) && cache.ContainsKey(magic.ToString() ?? string.Empty);

        public static bool Matches<T>(Span<byte> span, T magic) where T : struct => GetMagicValue<T>(span).Equals(magic);

        public static T GetMagicValue<T>(Span<byte> span) where T : struct => span.Length < 4 ? default : MemoryMarshal.Read<T>(span);
    }
}
