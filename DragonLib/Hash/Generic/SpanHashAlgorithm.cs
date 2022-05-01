using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace DragonLib.Hash.Generic;

public abstract class SpanHashAlgorithm<T> : HashAlgorithm where T : unmanaged, INumber<T> {
    protected SpanHashAlgorithm() {
        unsafe {
            HashSizeValue = sizeof(T) * 8;
        }
    }

    protected T Value { get; set; }

    protected override byte[] HashFinal() {
        Span<T> tmp = stackalloc T[1];
        tmp[0] = GetValueFinal();
        return MemoryMarshal.AsBytes(tmp).ToArray();
    }

    protected T ComputeHashValue(Span<byte> bytes) {
        HashCore(bytes.ToArray(), 0, bytes.Length);
        return GetValueFinal();
    }

    protected abstract T GetValueFinal();
}
