using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace DragonLib.Hash.Generic;

[RequiresPreviewFeatures]
public abstract class SpanHashAlgorithm<T> : HashAlgorithm 
#pragma warning disable CA2252
    where T : unmanaged, INumber<T> {
#pragma warning enable CA2252
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

    public T ComputeHashValue(Span<byte> bytes) {
        HashCore(bytes.ToArray(), 0, bytes.Length);
        return GetValueFinal();
    }

    protected abstract T GetValueFinal();
}
