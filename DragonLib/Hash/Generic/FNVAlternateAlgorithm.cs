using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonLib.Hash.Generic;

// https://tools.ietf.org/html/draft-eastlake-fnv-17
// http://www.isthe.com/chongo/tech/comp/fnv/index.html
public class FNVAlternateAlgorithm<T> : SpanHashAlgorithm<T> where T : unmanaged, INumber<T>, IBitwiseOperators<T, T, T> {
    private readonly T Basis;
    private readonly T Prime;

    public unsafe FNVAlternateAlgorithm(T basis, T prime) {
        Basis = basis;
        Prime = prime;
        HashSizeValue = sizeof(T) * 8;
        Reset();
    }
    
#if RELEASE
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    protected override void HashCore(byte[] array, int ibStart, int cbSize) {
        while (cbSize > 0) {
            Value ^= T.Create(array[ibStart++]);
            Value *= Prime;
            cbSize--;
        }
    }

    protected override byte[] HashFinal() {
        Span<T> tmp = stackalloc T[1];
        tmp[0] = Value;
        return MemoryMarshal.AsBytes(tmp).ToArray();
    }

#if RELEASE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void Reset() => Value = Basis;
    public override void Initialize() => Reset();
    protected override T GetValueFinal() => Value;
}
