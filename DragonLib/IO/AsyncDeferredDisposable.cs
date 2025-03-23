using System.Threading.Tasks;

namespace DragonLib.IO;

public sealed class AsyncDeferredDisposable(Func<Task> deferred) : IDisposable, IAsyncDisposable {
	public void Dispose() => deferred().Wait();

	public async ValueTask DisposeAsync() => await deferred();
}
