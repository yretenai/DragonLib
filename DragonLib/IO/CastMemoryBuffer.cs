using System.Buffers;
using System.Runtime.CompilerServices;

namespace DragonLib.IO;

public sealed class CastMemoryBuffer<T>(IWritableMemoryBuffer<byte> underlyingOwner, int byteOffset, int length) : IMemoryOwner<T> where T : struct {
	public CastMemoryBuffer(IWritableMemoryBuffer<byte> underlyingOwner, int offset) :
		this(underlyingOwner, offset, (underlyingOwner.Length - offset) / Unsafe.SizeOf<T>()) { }
	public CastMemoryBuffer(IWritableMemoryBuffer<byte> underlyingOwner) :
		this(underlyingOwner, 0, underlyingOwner.Length / Unsafe.SizeOf<T>()) { }

	~CastMemoryBuffer() => Dispose(false);

	public MemoryTypeManager<T, byte>? Manager { get; private set; } =
		new(underlyingOwner.Memory.Slice(byteOffset, length * Unsafe.SizeOf<T>()));

	public int Offset { get; set; }
	public int ByteOffset { get; set; } = byteOffset;
	public int Length { get; set; } = length;

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public Memory<T> Memory => Length <= 0 ? Memory<T>.Empty : Manager!.Memory;


	private void Dispose(bool _) {
		(Manager as IDisposable)?.Dispose();
		Manager = null;
	}
}
