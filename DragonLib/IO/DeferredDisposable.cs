// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO;

public sealed class DeferredDisposable(Action deferred) : IDisposable {
	public void Dispose() => deferred();
}
