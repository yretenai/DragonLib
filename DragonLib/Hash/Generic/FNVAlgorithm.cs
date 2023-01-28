using System.Numerics;
using System.Runtime.CompilerServices;

namespace DragonLib.Hash.Generic;

// https://tools.ietf.org/html/draft-eastlake-fnv-17
// http://www.isthe.com/chongo/tech/comp/fnv/index.html
public class FNVAlgorithm<T> : SpanHashAlgorithm<T>
    where T : unmanaged, INumber<T>, IBitwiseOperators<T, T, T> {
    public const string FNV1_IV = "chongo <Landon Curt Noll> /\\../\\";
    public const string FNV1B_IV = "chongo (Landon Curt Noll) /\\oo/\\";
    private readonly T Basis;
    private readonly T Prime;

    public FNVAlgorithm(T basis, T prime) {
        Basis = basis;
        Prime = prime;
        Reset(Basis);
    }

    public T HashNext(T value) {
        Value *= Prime;
        Value ^= value;
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

    public override void Initialize() => Reset(Basis);

    // there's some math behind the theory on selecting FNV primes, read more on it on either linked pages.
    // the IETF page also has sample C code for 128 and higher bit spaces.
    public static T CalculateBasis(string text = FNV1_IV, T prime = default) {
        using var hasher = new FNVAlgorithm<T>(default, prime);
        return hasher.ComputeHashValue(Encoding.ASCII.GetBytes(text));
    }

    protected override T GetValueFinal() => Value;
}
