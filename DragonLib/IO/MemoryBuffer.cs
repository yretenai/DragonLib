using System.Buffers;

namespace DragonLib.IO;

public sealed class MemoryBuffer(IMemoryOwner<byte> Memory, int Size) : IMemoryBuffer {
	public Span<byte> WritableSpan => Memory.Memory.Span[..Size];
	public int Length => Size;
	public ReadOnlySpan<byte> Span => WritableSpan;
	public byte this[int offset] => Span[offset];

	public void Dispose() => Memory.Dispose();
}
