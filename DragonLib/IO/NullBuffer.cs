namespace DragonLib.IO;

public sealed class NullBuffer<T> : IMemoryBuffer<T> where T : struct {
	public Span<T> Span => Span<T>.Empty;
	public Memory<T> Memory => Memory<T>.Empty;

	public int Length => 0;
	public T this[int offset] {
		get => Span[offset];
		set => Span[offset] = value;
	}

	public void Dispose() { }
}
