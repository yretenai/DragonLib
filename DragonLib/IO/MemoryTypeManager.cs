using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonLib.IO;

public class MemoryTypeManager<TTo, TFrom>(Memory<TFrom> buffer) : MemoryManager<TTo> where TTo : struct
																					  where TFrom : struct {
	private static readonly int ToSize = Unsafe.SizeOf<TTo>();
	private static readonly int FromSize = Unsafe.SizeOf<TFrom>();
	private MemoryHandle? Handle { get; set; }
	private int RefCount { get; set; }
	private Memory<TFrom> Buffer { get; set; } = buffer;
	private bool Disposed { get; set; }
	public int ByteSize { get; } = buffer.Length * FromSize;

	public override Span<TTo> GetSpan() {
		ObjectDisposedException.ThrowIf(Disposed, this);
		return MemoryMarshal.Cast<TFrom, TTo>(Buffer.Span);
	}

	public override MemoryHandle Pin(int elementIndex = 0) {
		ObjectDisposedException.ThrowIf(Disposed, this);
		if (elementIndex < 0) {
			throw new IndexOutOfRangeException();
		}

		var byteIndex = ToSize * elementIndex * FromSize;
		if (byteIndex >= ByteSize) {
			throw new IndexOutOfRangeException();
		}

		RefCount++;
		Handle ??= Buffer.Pin();

		unsafe {
			return new MemoryHandle(((nint) Handle.Value.Pointer + byteIndex).ToPointer(), default, this);
		}
	}

	public override void Unpin() {
		ObjectDisposedException.ThrowIf(Disposed, this);
		RefCount--;
		if (RefCount <= 0) {
			RefCount = 0;
			Handle?.Dispose();
			Handle = null;
		}
	}

	protected override bool TryGetArray(out ArraySegment<TTo> segment) {
		ObjectDisposedException.ThrowIf(Disposed, this);
		return MemoryMarshal.TryGetArray(Memory, out segment);
	}

	protected override void Dispose(bool disposing) {
		ObjectDisposedException.ThrowIf(Disposed, this);
		Disposed = true;
		Handle?.Dispose();
		Buffer = null!;
	}
}
