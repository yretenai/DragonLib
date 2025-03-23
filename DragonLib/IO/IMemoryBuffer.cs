using System.Buffers;

namespace DragonLib.IO;

public interface IMemoryBuffer<T> : IMemoryOwner<T> where T : struct {
	public static NullBuffer<T> Empty { get; } = new();

	public int Length { get; }
	public Span<T> Span { get; }
	public T this[int offset] { get; set; }
}
