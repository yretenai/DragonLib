using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace DragonLib
{
    [PublicAPI]
    public static class BitPacked
    {
        private static Dictionary<Type, List<(PropertyInfo, int, int)>> CachedBits { get; } = new Dictionary<Type, List<(PropertyInfo, int, int)>>();

        private static List<(PropertyInfo Property, int Offset, int Mask)> Preload<T>() where T : struct
        {
            // ReSharper disable once InvertIf
            if (!CachedBits.TryGetValue(typeof(T), out var cached))
            {
                cached = new List<(PropertyInfo, int, int)>();
                var type = typeof(T);
                var properties = type.GetMembers().OfType<PropertyInfo>().Where(x => x.GetMethod != null && x.SetMethod != null).ToArray();
                var offset = 0;
                foreach (var property in properties)
                {
                    var info = property.GetCustomAttribute<BitFieldAttribute>();
                    if (info == null || info.Length == 0) continue;
                    var mask = info.Length == 1 ? 1 : (1 << info.Length) - 1;
                    cached.Add((property, offset, mask));
                    offset += info.Length;
                }

                CachedBits[typeof(T)] = cached;
            }

            return cached;
        }

        public static uint Pack<T>(T instance) where T : struct
        {
            var packed = 0u;
            var properties = Preload<T>();
            object boxed = instance;
            foreach (var (property, offset, _) in properties)
            {
                var propertyValue = property.GetMethod?.Invoke(boxed, new object[0]);
                if (propertyValue == null) continue;
                var objectValue = Convert.ChangeType(propertyValue, TypeCode.UInt32);
                if (objectValue == null) continue;
                packed |= (uint) objectValue << offset;
            }

            return packed;
        }

        public static T Unpack<T>(uint value) where T : struct
        {
            var instance = Activator.CreateInstance(typeof(T));
            if (instance == null) return default;
            var properties = Preload<T>();
            foreach (var (property, offset, mask) in properties)
            {
                var propertyValue = value >> offset & mask;
                var intValue = Convert.ChangeType(propertyValue, property.PropertyType);
                property.SetValue(instance, intValue, null);
            }

            return (T) instance;
        }
    }
}
