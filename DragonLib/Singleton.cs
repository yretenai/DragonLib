// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics.CodeAnalysis;

namespace DragonLib;

public static class Singleton<T> where T : class, new() {
	[field: AllowNull] [field: MaybeNull]
	public static T Instance {
		get => field ??= new T();
		set;
	}
}
