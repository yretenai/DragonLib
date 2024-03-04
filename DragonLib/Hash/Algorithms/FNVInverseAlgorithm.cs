using System.Numerics;
using System.Runtime.CompilerServices;

namespace DragonLib.Hash.Algorithms;

// https://tools.ietf.org/html/draft-eastlake-fnv-17
// http://www.isthe.com/chongo/tech/comp/fnv/index.html
public sealed class FNVInverseAlgorithm<T> : SpanHashAlgorithm<T>
    where T : unmanaged, INumber<T>, IBitwiseOperators<T, T, T> {
    public const string FNV1_IV = "chongo <Landon Curt Noll> /\\../\\";
    public const string FNV1B_IV = "chongo (Landon Curt Noll) /\\oo/\\";
    private readonly T Basis;
    private readonly T Prime;

    public FNVInverseAlgorithm(T basis, T prime) {
        Basis = basis;
        Prime = prime;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset(T value) => Value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Reset() => Reset(Basis);

    public override void Initialize() => Reset(Basis);

    protected override T GetValueFinal() {
        var val = Value;
        Reset();
        return val;
    }
}
