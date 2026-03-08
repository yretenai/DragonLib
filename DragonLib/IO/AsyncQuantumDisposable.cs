// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;

namespace DragonLib.IO;

public sealed class AsyncQuantumDisposable<T> : IDisposable, IAsyncDisposable where T : class, IDisposable, IAsyncDisposable {
	public AsyncQuantumDisposable(T? disposable = null) => Disposable = disposable;

	public T? Disposable { get; set; }

	public async ValueTask DisposeAsync() {
		if (Disposable is not null) {
			await Disposable.DisposeAsync();
		}
	}

	public void Dispose() => Disposable?.Dispose();
}
