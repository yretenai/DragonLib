// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;

namespace DragonLib.IO;

public sealed class AsyncDeferredDisposable(Func<Task> deferred) : IDisposable, IAsyncDisposable {
	public async ValueTask DisposeAsync() => await deferred();
	public void Dispose() => deferred().Wait();
}
