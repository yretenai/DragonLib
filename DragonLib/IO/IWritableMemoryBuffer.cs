using System.Buffers;

namespace DragonLib.IO;

public interface IWritableMemoryBuffer<T> : IMemoryBuffer<T>, IMemoryOwner<T> where T : struct {
	public new Span<T> Span { get; }
	public new Memory<T> Memory { get; }
	public new T this[int offset] { get; set; }
}
