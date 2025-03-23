using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonLib.IO;

public class MemoryReader(IMemoryBuffer<byte> buffer, bool leaveOpen = false) : IDisposable {
	public int Offset { get; set; }
	public IMemoryBuffer<byte> Buffer { get; } = buffer;

	public T Read<T>() where T : struct {
		var value = MemoryMarshal.Read<T>(Buffer.Memory[Offset..].Span);
		Offset += Unsafe.SizeOf<T>();
		return value;
	}

	public ReadOnlySpan<T> Read<T>(int count) where T : struct {
		var value = Buffer.Span.Slice(Offset, count * Unsafe.SizeOf<T>());
		Offset += value.Length;
		return MemoryMarshal.Cast<byte, T>(value);
	}

	public void Read<T>(Span<T> storage) where T : struct => Read<T>(storage.Length).CopyTo(storage);

	public void ReadMemory<T>(Memory<T> storage) where T : struct => Read<T>(storage.Length).CopyTo(storage.Span);

	public BorrowedMemoryBuffer<byte> Partition(int count) {
		var value = new BorrowedMemoryBuffer<byte>(Buffer, Offset, count);
		Offset += value.Length;
		return value;
	}

	public CastMemoryBuffer<T, byte> Partition<T>(int count) where T : struct {
		var value = Partition(count * Unsafe.SizeOf<T>());
		return new CastMemoryBuffer<T, byte>(value, 0);
	}

	public string ReadString() => SpanReader.ReadString(Buffer.Span, Offset);

	public string ReadUTF8String() => SpanReader.ReadUTF8String(Buffer.Span, Offset);

	public string ReadString(int length) {
		var text = Encoding.ASCII.GetString(Buffer.Span.Slice(Offset, length));
		Offset += length;
		return text;
	}

	public string ReadUTF8String(int length) {
		var text = Encoding.UTF8.GetString(Buffer.Span.Slice(Offset, length));
		Offset += length;
		return text;
	}

	protected virtual void Dispose(bool disposing) {
		if (disposing) {
			if (!leaveOpen) {
				Buffer.Dispose();
			}
		}
	}

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
