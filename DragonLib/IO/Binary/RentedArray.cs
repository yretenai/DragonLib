// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonLib.IO.Binary;

public sealed class RentedArray<T> : IDisposable where T : struct {
	public RentedArray(T[] array, int length) {
		Array = array;
		Length = length;
	}

	public RentedArray(int length) : this(length == 0 ? [] : ArrayPool<T>.Shared.Rent(length), length) { }

	public RentedArray() {
		Length = 0;
		Array = [];
	}

	public T[] Array { get; private set; }
	public int Length { get; private set; }
	public ArraySegment<T> Segment => Length == 0 ? ArraySegment<T>.Empty : new ArraySegment<T>(Array, 0, Length);
	public Memory<T> Memory => Length == 0 ? Memory<T>.Empty : Array.AsMemory(0, Length);
	public Span<T> Span => Length == 0 ? Span<T>.Empty : Array.AsSpan(0, Length);

	public T this[int index] {
		get => Array[index];
		set => Array[index] = value;
	}

	public void Dispose() {
		if (Length == 0) {
			return;
		}

		ArrayPool<T>.Shared.Return(Array);
		Array = [];
		Length = 0;
	}

	public static RentedArray<T> FromFile(string path) => FromStream(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), false);

	private static RentedArray<T> FromStream(Stream stream, bool leaveOpen) {
		try {
			var buffer = new RentedArray<T>((int) (stream.Length - stream.Position) / Unsafe.SizeOf<T>());
			stream.ReadExactly(MemoryMarshal.AsBytes(buffer.Span));
			return buffer;
		} finally {
			if (!leaveOpen) {
				stream.Close();
				stream.Dispose();
			}
		}
	}
}
