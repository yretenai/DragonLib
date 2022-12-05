using System.Runtime.InteropServices;

namespace DragonLib.Unsafe;

public abstract class ArrayBase : IEquatable<ArrayBase> {
    public readonly int Length;
    protected nint Ptr;

    protected ArrayBase(int length) => Length = length;

    public bool Equals(ArrayBase? other) {
        if (ReferenceEquals(null, other)) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        return Ptr == other.Ptr && Length == other.Length;
    }

    public override bool Equals(object? obj) => obj is ArrayBase other && Equals(other);
    public override int GetHashCode() => Ptr.GetHashCode();
    public static bool operator ==(ArrayBase? left, ArrayBase? right) => Equals(left, right);
    public static bool operator !=(ArrayBase? left, ArrayBase? right) => !Equals(left, right);

    public virtual unsafe Array<T> Cast<T>() where T : unmanaged => Length == 0 ? Array<T>.Empty : new Array<T>(Ptr, Length, Array<T>.NullFree);
}

public unsafe delegate void FreeArrayDelegate(void* ptr);

public sealed class Array<T> : ArrayBase, IDisposable, IEnumerable<T>, IEquatable<Array<T>> where T : unmanaged {
    private readonly FreeArrayDelegate FreeArrayInner;

    private unsafe Array() : base(0) {
        Ptr = 0;
        FreeArrayInner = NullFree;
    }

    public Array(nint ptr, int length, FreeArrayDelegate freeArray) : base(length) {
        if (ptr == 0) {
            throw new ArgumentException("Pointer cannot be null", nameof(ptr));
        }

        if (length <= 0) {
            throw new ArgumentException("Length must be greater than 0", nameof(length));
        }

        Ptr = ptr;
        FreeArrayInner = freeArray;
    }

    public static Array<T> Empty { get; } = new();

    public bool IsEmpty => Length == 0 || Ptr == 0;

    public unsafe Span<T> Span {
        get {
            ThrowIfDisposed();

            return IsEmpty ? Span<T>.Empty : new Span<T>((T*) Ptr, Length);
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

    public ref T this[int index] => ref Get(index);

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

    public bool Equals(Array<T>? other) => base.Equals(other);

    public unsafe ref T Get(int index) {
        ThrowIfDisposed();

        if (index >= Length || index < 0) {
            throw new IndexOutOfRangeException();
        }

        return ref ((T*) Ptr)[index];
    }

    public unsafe void Set(int index, T value) {
        ThrowIfDisposed();

        if (index >= Length || index < 0) {
            throw new IndexOutOfRangeException();
        }


        ((T*) Ptr)[index] = value;
    }

    public static unsafe void NullFree(void* ptr) { }

    private void ThrowIfDisposed() {
        if (Ptr == 0 && Length != 0) {
            throw new ObjectDisposedException(nameof(Array<T>));
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

    public unsafe Array<T> ToAligned(int alignment = 16) {
        ThrowIfDisposed();

        if (IsEmpty || Ptr % alignment == 0) {
            return this;
        }

        var array = AllocAligned(Length, alignment);
        NativeMemory.Copy((void*) Ptr, (void*) array.Ptr, (uint) (Length * sizeof(T)));
        Dispose();
        return array;
    }

    ~Array() {
        Free();
    }

    public static unsafe Array<T> Alloc(int length) => length == 0 ? Empty : new Array<T>((nint) NativeMemory.Alloc((nuint) (length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>())), length, NativeMemory.Free);
    public static unsafe Array<T> AllocZeroed(int length) => length == 0 ? Empty : new Array<T>((nint) NativeMemory.AllocZeroed((nuint) (length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>())), length, NativeMemory.Free);
    public static unsafe Array<T> AllocAligned(int length, int alignment = 16) => length == 0 ? Empty : new Array<T>((nint) NativeMemory.AlignedAlloc((nuint) (length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>()), (nuint) alignment), length, NativeMemory.AlignedFree);
    // todo: public static NativeArray<T> AllocPool(int length, TLSFPool? pool = null) => length == 0 ? Empty : new Array<T>((pool ?? TLSFPool.Shared).Alloc((nuint) (length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>()), length, TLSFPool.Shared.Free); -- surely one day i'll make a tlsf arena in C#

    public unsafe void Free() {
        if (Ptr != 0) {
            FreeArrayInner((void*) Ptr);
            Ptr = 0;
        }
    }

    public override bool Equals(object? obj) => base.Equals(obj) && obj is Array<T> other && Equals(other);
    public override int GetHashCode() => base.GetHashCode();
    public static bool operator ==(Array<T>? left, Array<T>? right) => Equals(left, right);
    public static bool operator !=(Array<T>? left, Array<T>? right) => !Equals(left, right);
}
