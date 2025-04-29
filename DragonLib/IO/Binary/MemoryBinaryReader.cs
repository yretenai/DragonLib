namespace DragonLib.IO.Binary;

public class MemoryBinaryReader : BufferBinaryReader {
	public MemoryBinaryReader(Memory<byte> memory) => Memory = memory;

	public Memory<byte> Memory { get; }
	public override int Position { get; set; }

	public override void ReadBytes(Span<byte> span) => Memory.Span.Slice(Position, span.Length).CopyTo(span);

	protected override void Dispose(bool disposing) { }
}
