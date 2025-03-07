using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonLib.IO;

public class MemoryBufferReader(IMemoryBuffer Buffer) : IDisposable {
	public int Offset { get; set; }

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public T Read<T>() where T : struct {
		var value = MemoryMarshal.Read<T>(Buffer.Span[Offset..]);
		Offset += Unsafe.SizeOf<T>();
		return value;
	}

	public ReadOnlySpan<T> Read<T>(int count) where T : struct {
		var value = Buffer.Span.Slice(Offset, count * Unsafe.SizeOf<T>());
		Offset += value.Length;
		return MemoryMarshal.Cast<byte, T>(value);
	}

	public string ReadString() => SpanReader.ReadString(Buffer.Span, Offset);

	public string ReadUTF8String() => SpanReader.ReadUTF8String(Buffer.Span, Offset);

	protected virtual void Dispose(bool disposing) {
		if (disposing) {
			Buffer.Dispose();
		}
	}

	~MemoryBufferReader() => Dispose(false);
}
