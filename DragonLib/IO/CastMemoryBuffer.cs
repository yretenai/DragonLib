using System.Runtime.CompilerServices;

namespace DragonLib.IO;

public sealed class CastMemoryBuffer<T, TBase>(IMemoryBuffer<TBase> underlyingOwner, int byteOffset, int length) :
	IMemoryBuffer<T> where T : struct where TBase : struct {
	public CastMemoryBuffer(IMemoryBuffer<TBase> underlyingOwner, int offset) :
		this(underlyingOwner, offset, (underlyingOwner.Length - offset) / Unsafe.SizeOf<T>()) { }

	public CastMemoryBuffer(IMemoryBuffer<TBase> underlyingOwner) :
		this(underlyingOwner, 0, underlyingOwner.Length / Unsafe.SizeOf<T>()) { }

	public MemoryTypeManager<T, TBase>? Manager { get; private set; } =
		new(underlyingOwner.Memory.Slice(byteOffset, length * Unsafe.SizeOf<T>()));

	public int Offset { get; set; }
	public int ByteOffset { get; set; } = byteOffset;
	public int Length { get; set; } = length;

	public T this[int offset] {
		get => Span[offset];
		set => Span[offset] = value;
	}

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public Memory<T> Memory => Length <= 0 ? Memory<T>.Empty : Manager!.Memory;

	public Span<T> Span => Memory.Span;

	~CastMemoryBuffer() => Dispose(false);


	private void Dispose(bool _) {
		(Manager as IDisposable)?.Dispose();
		Manager = null;
	}
}
