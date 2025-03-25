namespace DragonLib.IO;

public sealed class BorrowedMemoryBuffer<T>(IMemoryBuffer<T> buffer, int length, int offset) : IMemoryBuffer<T> where T : struct {
	public int Offset { get; } = offset;
	public Memory<T> Memory { get; } = buffer.Memory.Slice(offset, length);
	public Span<T> Span => Memory.Span;
	public int Length { get; } = length;

	public T this[int offset] {
		get => Span[offset];
		set => Span[offset] = value;
	}

	public void Dispose() {
		// do nothing
	}
}
