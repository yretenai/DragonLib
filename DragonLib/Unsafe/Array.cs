using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonLib.Unsafe;

public sealed class Array<T> : IDisposable, IEnumerable<T>, IEquatable<Array<T>> where T : unmanaged {
    public delegate void FreeDelegate(nint ptr);

    public int Length;
    public nint Pointer;
    private readonly FreeDelegate FreeInner;

    private Array(nint pointer, int length, FreeDelegate free) {
        if (pointer == 0) {
            throw new ArgumentException("Pointer cannot be null", nameof(pointer));
        }

        Pointer = pointer;
        Length = length;
        FreeInner = free;
    }

    public unsafe Span<T> Span => new((T*)Pointer, Length);

    public unsafe T[] ToManaged() {
        var array = new T[Length];
        fixed (T* ptr = array) {
            System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(ptr, (void*)Pointer, (uint)(Length * sizeof(T)));
        }

        return array;
    }

    public unsafe ref T this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if (index >= Length || index < 0) {
                throw new InvalidOperationException();
            }

            return ref ((T*)Pointer)[index];
        }
    }

    public void Dispose() {
        Free();
        GC.SuppressFinalize(this);
    }

    public bool Equals(Array<T>? other) => other?.Pointer == Pointer && other.Length == Length;

    ~Array() {
        Free();
    }

    public static Array<T> Allocate(int length) => new(Marshal.AllocHGlobal(length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>()), length, Marshal.FreeHGlobal);
    public static Array<T> AllocateCTM(int length) => new(Marshal.AllocCoTaskMem(length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>()), length, Marshal.FreeCoTaskMem);
    // todo: public static NativeArray<T> PoolAllocate(int length) => new(TLSFPool.Shared.Allocate(length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>()), length, TLSFPool.Shared.Free); -- surely one day i'll make a tlsf arena in C#

    public void Free() {
        if (Pointer != 0) {
            FreeInner(Pointer);
            Pointer = 0;
            Length = 0;
        }
    }

    public IEnumerator<T> GetEnumerator() {
        for (var i = 0; i < Length; ++i) {
            yield return this[i];
        }
    }

    public override bool Equals(object? obj) {
        if (obj is Array<T> other) {
            return Equals(other);
        }

        return false;
    }

    public override int GetHashCode() => (int)Pointer;
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static bool operator ==(Array<T> left, Array<T> right) => left.Equals(right);

    public static bool operator !=(Array<T> left, Array<T> right) => !(left == right);
}

