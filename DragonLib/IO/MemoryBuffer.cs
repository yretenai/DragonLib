using System.Buffers;

namespace DragonLib.IO;

public sealed class MemoryBuffer<T>(IMemoryOwner<T> buffer, int length) : IMemoryBuffer<T> where T : struct {
	public MemoryBuffer(int length) : this(MemoryPool<T>.Shared.Rent(length), length) { }

	public Memory<T> Memory { get; } = buffer.Memory[..length];

	public Span<T> Span => Memory.Span;

	public int Length { get; } = length;
	public T this[int offset] {
		get => Span[offset];
		set => Span[offset] = value;
	}

	public void Dispose() => buffer.Dispose();
}
