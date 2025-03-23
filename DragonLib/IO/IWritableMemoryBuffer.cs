namespace DragonLib.IO;

public interface IWritableMemoryBuffer : IMemoryBuffer {
	public new Span<byte> Span { get; }
	public new Memory<byte> Memory { get; }
}
