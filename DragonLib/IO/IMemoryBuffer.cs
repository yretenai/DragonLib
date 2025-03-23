namespace DragonLib.IO;

public interface IMemoryBuffer : IDisposable {
	public static NullBuffer Empty { get; } = new();

	public int Length { get; }
	public ReadOnlySpan<byte> Span { get; }
	public ReadOnlyMemory<byte> Memory { get; }
	public byte this[int offset] { get; }
}
