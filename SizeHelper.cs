using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace DragonLib
{
    public static class SizeHelper
    {
        private static ConcurrentDictionary<Type, int> SizeCache { get; } = new ConcurrentDictionary<Type, int>();

        public static int SizeOf<T>()
        {
            var type = typeof(T);
            if (SizeCache.TryGetValue(type, out var size)) return size;
            size = Unsafe.SizeOf<T>();
            SizeCache[type] = size;
            return size;
        }
    }
}
