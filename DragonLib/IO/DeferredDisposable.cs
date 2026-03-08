// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO;

public sealed class DeferredDisposable(Action deferred) : IDisposable {
	public void Dispose() => deferred();
}
