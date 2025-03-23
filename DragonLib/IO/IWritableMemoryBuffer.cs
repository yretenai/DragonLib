using System.Buffers;

namespace DragonLib.IO;

public interface IWritableMemoryBuffer : IMemoryBuffer, IMemoryOwner<byte> {
	public new Span<byte> Span { get; }
	public new Memory<byte> Memory { get; }
}
