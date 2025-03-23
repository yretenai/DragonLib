namespace DragonLib.IO;

public interface IMemoryBuffer<T> : IDisposable where T : struct {
	public static NullBuffer<T> Empty { get; } = new();

	public int Length { get; }
	public ReadOnlySpan<T> Span { get; }
	public ReadOnlyMemory<T> Memory { get; }
	public T this[int offset] { get; }
}
