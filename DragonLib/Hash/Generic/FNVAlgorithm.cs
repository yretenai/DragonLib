namespace DragonLib.Hash.Generic;

// https://tools.ietf.org/html/draft-eastlake-fnv-17
// http://www.isthe.com/chongo/tech/comp/fnv/index.html
public class FNVAlgorithm<T> : SpanHashAlgorithm<T> where T : unmanaged, INumber<T>, IBitwiseOperators<T, T, T> {
    public const string FNV0_IV = "chongo (Landon Curt Noll) /\\oo/\\";
    public const string FNV1_IV = "chongo <Landon Curt Noll> /\\../\\";
    private readonly T Basis;
    private readonly T Prime;

    public FNVAlgorithm(T basis, T prime) {
        Basis = basis;
        Prime = prime;
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize) {
        while (cbSize > 0) {
            Value *= Prime;
            Value ^= T.Create(array[ibStart++]);
            cbSize--;
        }
    }

    public override void Initialize() => Value = Basis;

    // there's some math behind the theory on selecting FNV primes, read more on it on either linked pages.
    // the IETF page also has sample C code for 128 and higher bit spaces.
    public static T CalculateBasis(string text = FNV1_IV, T prime = default) {
        using var hasher = new FNVAlgorithm<T>(default, prime);
        return hasher.ComputeHashValue(Encoding.ASCII.GetBytes(text));
    }
}
