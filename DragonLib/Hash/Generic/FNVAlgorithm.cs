using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace DragonLib.Hash.Generic;

// https://tools.ietf.org/html/draft-eastlake-fnv-17
// http://www.isthe.com/chongo/tech/comp/fnv/index.html
[RequiresPreviewFeatures]
public class FNVAlgorithm<T> : SpanHashAlgorithm<T> 
#pragma warning disable CA2252
    where T : unmanaged, INumber<T>, IBitwiseOperators<T, T, T> { 
#pragma warning enable CA2252
    public const string FNV1_IV = "chongo <Landon Curt Noll> /\\../\\";
    public const string FNV1B_IV = "chongo (Landon Curt Noll) /\\oo/\\";
    private readonly T Basis;
    private readonly T Prime;

    public FNVAlgorithm(T basis, T prime) {
        Basis = basis;
        Prime = prime;
        Reset();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    protected override void HashCore(byte[] array, int ibStart, int cbSize) {
        while (cbSize > 0) {
            Value *= Prime;
            Value ^= T.Create(array[ibStart++]);
            cbSize--;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset() => Value = Basis;

    public override void Initialize() => Reset();

    // there's some math behind the theory on selecting FNV primes, read more on it on either linked pages.
    // the IETF page also has sample C code for 128 and higher bit spaces.
    public static T CalculateBasis(string text = FNV1_IV, T prime = default) {
        using var hasher = new FNVAlgorithm<T>(default, prime);
        return hasher.ComputeHashValue(Encoding.ASCII.GetBytes(text));
    }

    protected override T GetValueFinal() => Value;
}
