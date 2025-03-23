namespace DragonLib.IO;

public sealed class BorrowedMemoryBuffer<T>(IMemoryBuffer<T> buffer, int length, int offset) : IWritableMemoryBuffer<T> where T : struct {
	public Memory<T> Memory { get; } = buffer is IWritableMemoryBuffer<T> writable
		? writable.Memory.Slice(offset, length)
		: Memory<T>.Empty;

	public Span<T> Span => Memory.Span;
	ReadOnlyMemory<T> IMemoryBuffer<T>.Memory => Memory;
	ReadOnlySpan<T> IMemoryBuffer<T>.Span => Span;

	public int Offset { get; } = offset;
	public int Length { get; } = length;
	public T this[int offset] {
		get => Span[offset];
		set => Span[offset] = value;
	}

	public void Dispose() {
		// do nothing
	}
}
