using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonLib.IO;

public ref struct SpanReader(ReadOnlySpan<byte> buffer) {
	public ReadOnlySpan<byte> Buffer { get; } = buffer;
	public int Offset { get; set; }

	public T Read<T>() where T : struct {
		var value = MemoryMarshal.Read<T>(Buffer[Offset..]);
		Offset += Unsafe.SizeOf<T>();
		return value;
	}

	public ReadOnlySpan<T> Read<T>(int count) where T : struct {
		var value = Buffer.Slice(Offset, count * Unsafe.SizeOf<T>());
		Offset += value.Length;
		return MemoryMarshal.Cast<byte, T>(value);
	}

	public string ReadString() => ReadString(Buffer, Offset);

	public string ReadUTF8String() => ReadUTF8String(Buffer, Offset);

	public string ReadString(int length) {
		var text = Encoding.ASCII.GetString(Buffer.Slice(Offset, length));
		Offset += length;
		return text;
	}

	public string ReadUTF8String(int length) {
		var text = Encoding.UTF8.GetString(Buffer.Slice(Offset, length));
		Offset += length;
		return text;
	}

	public static string ReadString(ReadOnlySpan<byte> strings, int index) {
		strings = strings[index..];
		var nul = strings.IndexOf((byte) 0);
		if (nul == -1) {
			nul = strings.Length;
		}

		return Encoding.ASCII.GetString(strings[..nul]);
	}

	public static string ReadUTF8String(ReadOnlySpan<byte> strings, int index) {
		strings = strings[index..];
		var nul = strings.IndexOf((byte) 0);
		if (nul == -1) {
			nul = strings.Length;
		}

		return Encoding.UTF8.GetString(strings[..nul]);
	}
}
