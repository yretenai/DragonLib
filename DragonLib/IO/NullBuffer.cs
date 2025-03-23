namespace DragonLib.IO;

public sealed class NullBuffer : IWritableMemoryBuffer {
	public Span<byte> Span => Span<byte>.Empty;
	public Memory<byte> Memory => Memory<byte>.Empty;
	ReadOnlyMemory<byte> IMemoryBuffer.Memory => Memory;
	ReadOnlySpan<byte> IMemoryBuffer.Span => Span;

	public int Length => 0;
	public byte this[int offset] => 0;

	public void Dispose() { }
}
