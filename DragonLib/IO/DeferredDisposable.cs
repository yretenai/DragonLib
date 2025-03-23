namespace DragonLib.IO;

public sealed class DeferredDisposable(Action deferred) : IDisposable {
	public void Dispose() => deferred();
}
