using System.Threading.Tasks;

namespace DragonLib.IO;

public sealed class NullDisposable : IDisposable, IAsyncDisposable {
	public static NullDisposable Instance { get; } = new();
	public ValueTask DisposeAsync() => ValueTask.CompletedTask;

	public void Dispose() { }
}
