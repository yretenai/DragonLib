using System.Buffers;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace DragonLib.Unsafe;

public abstract class RuntimeArrayBase : IEquatable<RuntimeArrayBase> {
    protected static unsafe FreeArrayDelegate NullFree { get; } = _ => { };
    internal static unsafe FreeArrayDelegate NativeFree { get; } = NativeMemory.Free;
    internal static unsafe FreeArrayDelegate NativeAlignedFree { get; } = NativeMemory.AlignedFree;

    public long Length { get; protected init; }
    protected nint Ptr { get; init; }
    protected bool Freed { get; set; }

    protected RuntimeArrayBase(long length) => Length = length;

    public bool Equals(RuntimeArrayBase? other) {
        if (ReferenceEquals(null, other)) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        return Ptr == other.Ptr && Length == other.Length;
    }

    public override bool Equals(object? obj) => obj is RuntimeArrayBase other && Equals(other);
    public override int GetHashCode() => Ptr.GetHashCode();
    public static bool operator ==(RuntimeArrayBase? left, RuntimeArrayBase? right) => Equals(left, right);
    public static bool operator !=(RuntimeArrayBase? left, RuntimeArrayBase? right) => !Equals(left, right);

    public virtual RuntimeArray<T> Cast<T>() where T : unmanaged => Length == 0 ? RuntimeArray<T>.Empty : new RuntimeArray<T>(Ptr, Length, NullFree);
}

public unsafe delegate void FreeArrayDelegate(void* ptr);
public unsafe delegate void FreeArrayCarryDelegate(void* ptr, object[] carry);

public sealed class RuntimeArray<T> : RuntimeArrayBase, IDisposable, IEnumerable<T>, IEquatable<RuntimeArray<T>> where T : unmanaged {
    private readonly FreeArrayDelegate FreeArrayInner;
    private readonly FreeArrayCarryDelegate? FreeArrayCarryInner;
    private readonly object[] Carry = Array.Empty<object>();

    private RuntimeArray() : base(0) {
        Ptr = 0;
        FreeArrayInner = NullFree;
    }

    public RuntimeArray(nint ptr, long length, FreeArrayDelegate freeArray) : base(length) {
        if (ptr == 0) {
            throw new ArgumentException("Pointer cannot be null", nameof(ptr));
        }

        if (length <= 0) {
            throw new ArgumentException("Length must be greater than 0", nameof(length));
        }

        Ptr = ptr;
        FreeArrayInner = freeArray;
    }

    public unsafe RuntimeArray(nint ptr, long length, FreeArrayCarryDelegate freeArray, params object[] carry) : base(length) {
        if (ptr == 0) {
            throw new ArgumentException("Pointer cannot be null", nameof(ptr));
        }

        if (length <= 0) {
            throw new ArgumentException("Length must be greater than 0", nameof(length));
        }

        Ptr = ptr;
        FreeArrayCarryInner = freeArray;
        Carry = carry;
        FreeArrayInner = FreeArrayInnerWithCarry;
    }

    public static RuntimeArray<T> Empty { get; } = new();

    public bool IsEmpty => Length == 0 || Ptr == 0;

    public unsafe Span<T> Span {
        get {
            ThrowIfDisposed();

            return IsEmpty ? Span<T>.Empty : new Span<T>((T*) Ptr, (int) Length);
        }
    }

    public unsafe T* Pointer {
        get {
            ThrowIfDisposed();

            if (IsEmpty) {
                return null;
            }

            return (T*) Ptr;
        }
    }

    public Memory<T> Memory => ToManaged().AsMemory();

    public ref T this[in int index] => ref Get(index);

    public void Dispose() {
        Free();
        GC.SuppressFinalize(this);
    }

    public IEnumerator<T> GetEnumerator() {
        ThrowIfDisposed();

        for (var i = 0; i < Length; ++i) {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(RuntimeArray<T>? other) => base.Equals(other);

    public unsafe ref T Get(in int index) {
        ThrowIfDisposed();

        if (index >= Length || index < 0) {
            throw new IndexOutOfRangeException();
        }

        return ref ((T*) Ptr)[index];
    }

    public unsafe void Set(in int index, in T value) {
        ThrowIfDisposed();

        if (index >= Length || index < 0) {
            throw new IndexOutOfRangeException();
        }

        ((T*) Ptr)[index] = value;
    }

    private void ThrowIfDisposed() {
        if (Freed) {
            throw new ObjectDisposedException(nameof(RuntimeArray<T>));
        }
    }

    public unsafe T[] ToManaged() {
        ThrowIfDisposed();

        if (IsEmpty) {
            return Array.Empty<T>();
        }

        var array = new T[Length];
        fixed (T* ptr = array) {
            NativeMemory.Copy((void*) Ptr, ptr, (uint) (Length * sizeof(T)));
        }

        return array;
    }

    public unsafe IMemoryOwner<byte> ToMemoryPool() {
        ThrowIfDisposed();

        if (IsEmpty) {
            return MemoryPool<byte>.Shared.Rent(0);
        }

        var array = MemoryPool<byte>.Shared.Rent((int)(Length * sizeof(T)));
        try {
            using var pin = array.Memory.Pin();
            NativeMemory.Copy((void*) Ptr, pin.Pointer, (uint) (Length * sizeof(T)));
            return array;
        } catch {
            array.Dispose();
            throw;
        }
    }

    public unsafe RuntimeArray<T> ToAligned(int alignment = 16) {
        ThrowIfDisposed();

        if (IsEmpty || Ptr % alignment == 0) {
            return this;
        }

        var array = AllocAligned((int) Length, alignment);
        NativeMemory.Copy((void*) Ptr, (void*) array.Ptr, (uint) (Length * sizeof(T)));
        Dispose();
        return array;
    }

    ~RuntimeArray() {
        Free();
    }

    public static unsafe RuntimeArray<T> Alloc(int length) => length == 0 ? Empty : new RuntimeArray<T>((nint) NativeMemory.Alloc((nuint) (length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>())), length, NativeFree);
    public static unsafe RuntimeArray<T> AllocZeroed(int length) => length == 0 ? Empty : new RuntimeArray<T>((nint) NativeMemory.AllocZeroed((nuint) (length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>())), length, NativeFree);
    public static unsafe RuntimeArray<T> AllocAligned(int length, int alignment = 16) => length == 0 ? Empty : new RuntimeArray<T>((nint) NativeMemory.AlignedAlloc((nuint) (length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>()), (nuint) alignment), length, NativeAlignedFree);
    // todo: public static NativeArray<T> AllocPool(int length, TLSFPool? pool = null) => length == 0 ? Empty : new Array<T>((pool ?? TLSFPool.Shared).Alloc((nuint) (length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>()), length, TLSFPool.Shared.Free); -- surely one day i'll make a tlsf arena in C#
    public static RuntimeArray<T> AllocFile(FileInfo fi) {
        using var fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        if (fs.Length > int.MaxValue) {
            return Empty;
        }

        var array = Alloc((int) fs.Length);
        fs.ReadExactly(array.Span.AsBytes());
        return array;
    }

    public static unsafe RuntimeArray<T> AllocFileMapped(FileInfo fi) {
        var fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var mmap = MemoryMappedFile.CreateFromFile(fs, fi.Name, fs.Length, MemoryMappedFileAccess.Read, HandleInheritability.Inheritable, false);
        byte* ptr = null;
        var view = mmap.CreateViewAccessor(0, fs.Length, MemoryMappedFileAccess.Read);
        view.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        var array = new RuntimeArray<T>((nint) ptr, fs.Length, FreeMemoryMap, mmap, view, fs);
        return array;
    }

    public override bool Equals(object? obj) => base.Equals(obj) && obj is RuntimeArray<T> other && Equals(other);
    public override int GetHashCode() => base.GetHashCode();
    public static bool operator ==(RuntimeArray<T>? left, RuntimeArray<T>? right) => Equals(left, right);
    public static bool operator !=(RuntimeArray<T>? left, RuntimeArray<T>? right) => !Equals(left, right);

    public unsafe void Free() {
        if (Freed && Ptr != 0) {
            FreeArrayInner((void*) Ptr);
            Freed = true;
        }
    }

    private unsafe void FreeArrayInnerWithCarry(void* ptr) {
        FreeArrayCarryInner?.Invoke(ptr, Carry);
    }

    private static unsafe void FreeMemoryMap(void* ptr, object[] carry) {
        if (carry.Length < 2) {
            return;
        }

        var view = (MemoryMappedViewAccessor) carry[1];
        view.SafeMemoryMappedViewHandle.ReleasePointer();
        FreeDispose(ptr, carry);
    }

    private static unsafe void FreeDispose(void* _, IEnumerable<object> carry) {
        foreach (var obj in carry) {
            if (obj is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
