// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Concurrent;

namespace DragonLib;

public static class ObjectPool<T> where T : class, new() {
	private static ConcurrentStack<T> Objects { get; } = [];

	public static T Rent() => Objects.TryPop(out var obj) ? obj : new T();
	public static T Rent(Func<T> factory) => Objects.TryPop(out var obj) ? obj : factory();

	public static void Return(T? obj) {
		if (obj == null) {
			return;
		}

		Objects.Push(obj);
	}
}
