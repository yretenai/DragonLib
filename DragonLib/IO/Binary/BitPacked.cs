// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Reflection;

namespace DragonLib.IO.Binary;

public static class BitPacked {
	private static Dictionary<Type, List<(PropertyInfo, int, ulong)>> CachedBits { get; } = new();

	private static List<(PropertyInfo Property, int Offset, ulong Mask)> Preload<T>() where T : struct {
		// ReSharper disable once InvertIf
		if (!CachedBits.TryGetValue(typeof(T), out var cached)) {
			cached = [];
			var type = typeof(T);
			var properties = type.GetMembers()
								 .OfType<PropertyInfo>()
								 .Where(x => x.GetMethod is not null && x.SetMethod is not null)
								 .ToArray();
			var offset = 0;
			foreach (var property in properties) {
				var info = property.GetCustomAttribute<BitFieldAttribute>();
				if (info is null || info.Length == 0) {
					continue;
				}

				var mask = info.Length == 1 ? 1UL : (1UL << info.Length) - 1;
				cached.Add((property, offset, mask));
				offset += info.Length;
			}

			CachedBits[typeof(T)] = cached;
		}

		return cached;
	}

	public static ulong Pack<T>(T instance) where T : struct {
		var packed = 0UL;
		var properties = Preload<T>();
		object boxed = instance;
		foreach (var (property, offset, _) in properties) {
			var propertyValue = property.GetMethod?.Invoke(boxed, []);
			if (propertyValue is null) {
				continue;
			}

			var objectValue = (ulong) Convert.ChangeType(propertyValue, TypeCode.UInt64);
			packed |= objectValue << offset;
		}

		return packed;
	}

	public static T Unpack<T>(ulong value) where T : struct {
		var instance = Activator.CreateInstance<T>();
		var properties = Preload<T>();
		foreach (var (property, offset, mask) in properties) {
			var propertyValue = (value >> offset) & mask;
			var type = property.PropertyType;
			var isEnum = type.IsEnum;
			if (isEnum) {
				type = type.GetEnumUnderlyingType();
			}

			var intValue = Convert.ChangeType(propertyValue, type);
			if (isEnum) {
				intValue = Enum.ToObject(property.PropertyType, intValue);
			}

			property.SetValue(instance, intValue, default);
		}

		return instance;
	}
}
