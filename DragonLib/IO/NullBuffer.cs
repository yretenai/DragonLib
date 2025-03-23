namespace DragonLib.IO;

public sealed class NullBuffer<T> : IWritableMemoryBuffer<T> where T : struct {
	public Span<T> Span => Span<T>.Empty;
	public Memory<T> Memory => Memory<T>.Empty;
	ReadOnlyMemory<T> IMemoryBuffer<T>.Memory => Memory;
	ReadOnlySpan<T> IMemoryBuffer<T>.Span => Span;

	public int Length => 0;
	public T this[int offset] {
		get => Span[offset];
		set => Span[offset] = value;
	}

	public void Dispose() { }
}
