// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;

namespace DragonLib.IO;

public sealed class NullDisposable : IDisposable, IAsyncDisposable {
	public static NullDisposable Instance { get; } = new();
	public ValueTask DisposeAsync() => ValueTask.CompletedTask;

	public void Dispose() { }
}
