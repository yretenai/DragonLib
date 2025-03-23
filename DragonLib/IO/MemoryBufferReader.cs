using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonLib.IO;

public class MemoryBufferReader(IMemoryBuffer<byte> buffer) : IDisposable {
	public int Offset { get; set; }

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public T Read<T>() where T : struct {
		var value = MemoryMarshal.Read<T>(buffer.Span[Offset..]);
		Offset += Unsafe.SizeOf<T>();
		return value;
	}

	public ReadOnlySpan<T> Read<T>(int count) where T : struct {
		var value = buffer.Span.Slice(Offset, count * Unsafe.SizeOf<T>());
		Offset += value.Length;
		return MemoryMarshal.Cast<byte, T>(value);
	}

	public void Read<T>(Span<T> storage) where T : struct => Read<T>(storage.Length).CopyTo(storage);

	public string ReadString() => SpanReader.ReadString(buffer.Span, Offset);

	public string ReadUTF8String() => SpanReader.ReadUTF8String(buffer.Span, Offset);

	protected virtual void Dispose(bool disposing) {
		if (disposing) {
			buffer.Dispose();
		}
	}

	~MemoryBufferReader() => Dispose(false);
}
