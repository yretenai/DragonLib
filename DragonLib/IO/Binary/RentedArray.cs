// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonLib.IO.Binary;

public interface IRentedArray<T> : IEnumerable<T>, IDisposable where T : struct {
	int Length { get; }
	Memory<T> Memory { get; }
	Span<T> Span { get; }
	T this[int Index] { get; set; }
}

public sealed class UnownedRentedArray<T> : IRentedArray<T> where T : struct {
	public UnownedRentedArray(IRentedArray<T> inner, int offset, int length) {
		Inner = inner;
		Offset = offset;
		Length = length;
	}

	public IRentedArray<T> Inner { get; }
	public int Offset { get; }
	public int Length { get; private set; }
	public Memory<T> Memory => Length == 0 ? Memory<T>.Empty : Inner.Memory.Slice(Offset, Length);
	public Span<T> Span => Length == 0 ? Span<T>.Empty : Inner.Span.Slice(Offset, Length);

	public T this[int index] {
		get => Inner[Offset + index];
		set => Inner[Offset + index] = value;
	}

	public void Dispose() {
		if (Length == 0) {
			return;
		}

		Length = 0;
	}

	public IEnumerator<T> GetEnumerator() {
		for (var i = 0; i < Length; ++i) {
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public sealed class UnownedCovariantArray<T> : IRentedArray<T> where T : struct {
	private class MemoryCastManager(Memory<byte> from) : MemoryManager<T> {
		private Memory<byte> From { get; } = from;

		public override Span<T> GetSpan() => MemoryMarshal.Cast<byte, T>(From.Span);
		protected override void Dispose(bool disposing) { }
		public override MemoryHandle Pin(int elementIndex = 0) => throw new NotSupportedException();
		public override void Unpin() => throw new NotSupportedException();
	}

	public UnownedCovariantArray(IRentedArray<byte> inner, int byteOffset, int length) {
		Inner = inner;
		Offset = byteOffset;
		Length = length;
		Manager = new MemoryCastManager(Inner.Memory.Slice(Offset, ByteLength));
	}

	private MemoryCastManager Manager { get; }
	public IRentedArray<byte> Inner { get; }
	public int Offset { get; }
	public int Length { get; private set; }
	public int ByteLength => Length * Unsafe.SizeOf<T>();
	public Memory<T> Memory => Length == 0 ? Memory<T>.Empty : Manager.Memory;
	public Span<T> Span => Length == 0 ? Span<T>.Empty : MemoryMarshal.Cast<byte, T>(Inner.Span.Slice(Offset, ByteLength));

	public T this[int index] {
		get => Span[index];
		set => Span[index] = value;
	}

	public void Dispose() {
		if (Length == 0) {
			return;
		}

		Length = 0;
	}

	public IEnumerator<T> GetEnumerator() {
		var slice = Memory;
		for (var i = 0; i < Length; ++i) {
			yield return slice.Span[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public sealed class RentedArray<T> : IRentedArray<T> where T : struct {
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

	public IEnumerator<T> GetEnumerator() {
		for (var i = 0; i < Length; ++i) {
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
