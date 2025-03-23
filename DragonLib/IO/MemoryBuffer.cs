using System.Buffers;

namespace DragonLib.IO;

public sealed class MemoryBuffer(IMemoryOwner<byte> buffer, int size) : IWritableMemoryBuffer {
	public MemoryBuffer(int size) : this(MemoryPool<byte>.Shared.Rent(size), size) { }
	public Span<byte> Span => Memory.Span;
	public Memory<byte> Memory => buffer.Memory[..size];
	ReadOnlyMemory<byte> IMemoryBuffer.Memory => Memory;
	ReadOnlySpan<byte> IMemoryBuffer.Span => Span;
	
	public int Length => size;
	public byte this[int offset] => Span[offset];

	public void Dispose() => buffer.Dispose();
}
