using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonLib.Hash.Algorithms;

// https://tools.ietf.org/html/draft-eastlake-fnv-17
// http://www.isthe.com/chongo/tech/comp/fnv/index.html
public class FNVInverseAlgorithm<T> : SpanHashAlgorithm<T>
    where T : unmanaged, INumber<T>, IBitwiseOperators<T, T, T> {
    private readonly T Basis;
    private readonly T Prime;

    public unsafe FNVInverseAlgorithm(T basis, T prime) {
        Basis = basis;
        Prime = prime;
        HashSizeValue = sizeof(T) * 8;
        Reset(Basis);
    }

    public T HashNext(T value) {
        Value ^= value;
        Value *= Prime;
        return Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    protected override void HashCore(byte[] array, int ibStart, int cbSize) {
        while (cbSize > 0) {
            HashNext(T.CreateSaturating(array[ibStart++]));
            cbSize--;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void HashCore(ushort[] array, int ibStart, int cbSize) {
        while (cbSize > 0) {
            HashNext(T.CreateSaturating(array[ibStart++]));
            cbSize--;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void HashCore(uint[] array, int ibStart, int cbSize) {
        while (cbSize > 0) {
            HashNext(T.CreateSaturating(array[ibStart++]));
            cbSize--;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void HashCore(ulong[] array, int ibStart, int cbSize) {
        while (cbSize > 0) {
            HashNext(T.CreateSaturating(array[ibStart++]));
            cbSize--;
        }
    }

    protected override byte[] HashFinal() {
        Span<T> tmp = stackalloc T[1];
        tmp[0] = Value;
        return MemoryMarshal.AsBytes(tmp).ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset(T value) => Value = value;

    public override void Initialize() => Reset(Basis);
    protected override T GetValueFinal() => Value;
}
