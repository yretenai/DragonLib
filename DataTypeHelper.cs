using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace DragonLib
{
    public static class DataTypeHelper
    {
        public static Dictionary<Type, Dictionary<object, string>> Cache { get; } = new Dictionary<Type, Dictionary<object, string>>();

        public static void Preload<T>() where T : struct
        {
            var type = typeof(T);
            var values = Enum.GetValues(type);
            Cache[type] = new Dictionary<object, string>();
            for (var i = 0; i < values.Length; ++i)
            {
                var value = values.GetValue(i);
                var extension = value.ToString().ToLower();
                var custom = type.GetMember(value.ToString()).SelectMany(x => x.GetCustomAttributes(false)).ToArray();
                if (custom.Any(x => x is FileExtensionAttribute)) extension = ((FileExtensionAttribute) custom.First(x => x is FileExtensionAttribute)).Extension;
                Cache[type][value] = extension;
            }
        }

        public static string GetExtension<T>(T magic) where T : struct
        {
            var type = typeof(T);
            if (!Cache.TryGetValue(type, out var cache)) return "bin";

            return cache.TryGetValue(magic, out var value) ? value : "bin";
        }

        public static bool IsKnown<T>(T magic) where T : struct
        {
            if (!Cache.TryGetValue(typeof(T), out var cache)) return false;

            return cache.ContainsKey(magic);
        }

        public static bool Matches<T>(Span<byte> span, T magic) where T : struct => GetMagicValue<T>(span).Equals(magic);

        public static T GetMagicValue<T>(Span<byte> span) where T : struct
        {
            if (span.Length < 4) return default;

            return MemoryMarshal.Read<T>(span);
        }
    }
}
