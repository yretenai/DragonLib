using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonLib.Unsafe;

public sealed class Array<T> : IDisposable, IEnumerable<T>, IEquatable<Array<T>> where T : unmanaged {
    public unsafe delegate void FreeDelegate(void* ptr);

    private readonly FreeDelegate FreeInner;

    public int Length;
    public nint Pointer;

    private Array(nint pointer, int length, FreeDelegate free) {
        if (pointer == 0) {
            throw new ArgumentException("Pointer cannot be null", nameof(pointer));
        }

        Pointer = pointer;
        Length = length;
        FreeInner = free;
    }

    public unsafe Span<T> Span => new((T*)Pointer, Length);

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

    public IEnumerator<T> GetEnumerator() {
        for (var i = 0; i < Length; ++i) {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(Array<T>? other) => other?.Pointer == Pointer && other.Length == Length;

    public unsafe T[] ToManaged() {
        var array = new T[Length];
        fixed (T* ptr = array) {
            NativeMemory.Copy((void*)Pointer, ptr, (uint)(Length * sizeof(T)));
        }

        return array;
    }

    ~Array() {
        Free();
    }

    public static unsafe Array<T> Alloc(int length) => new((nint)NativeMemory.Alloc((nuint)(length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>())), length, NativeMemory.Free);
    public static unsafe Array<T> AllocZeroed(int length) => new((nint)NativeMemory.AllocZeroed((nuint)(length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>())), length, NativeMemory.Free);
    public static unsafe Array<T> AllocAligned(int length, int alignment = 16) => new((nint)NativeMemory.AlignedAlloc((nuint)(length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>()), (nuint)alignment), length, NativeMemory.AlignedFree);
    // todo: public static NativeArray<T> AllocPool(int length, TLSFPool? pool = null) => new((pool ?? TLSFPool.Shared).Alloc((nuint)(length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>()), length, TLSFPool.Shared.Free); -- surely one day i'll make a tlsf arena in C#

    public unsafe void Free() {
        if (Pointer != 0) {
            FreeInner((void*)Pointer);
            Pointer = 0;
            Length = 0;
        }
    }

    public override bool Equals(object? obj) {
        if (obj is Array<T> other) {
            return Equals(other);
        }

        return false;
    }

    public override int GetHashCode() => (int)Pointer;

    public static bool operator ==(Array<T> left, Array<T> right) => left.Equals(right);

    public static bool operator !=(Array<T> left, Array<T> right) => !(left == right);
}
