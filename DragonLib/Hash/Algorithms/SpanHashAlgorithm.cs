using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace DragonLib.Hash.Algorithms;

public abstract class SpanHashAlgorithm<T> : HashAlgorithm
	where T : unmanaged, INumber<T> {
	protected SpanHashAlgorithm() {
		unsafe {
			HashSizeValue = sizeof(T) * 8;
		}
	}

	public T Value { get; set; }

	public abstract void Reset();

	protected override byte[] HashFinal() {
		Span<T> tmp = stackalloc T[1];
		tmp[0] = GetValueFinal();
		Reset();
		return MemoryMarshal.AsBytes(tmp).ToArray();
	}

	public virtual T ComputeHashValue(Span<byte> bytes) {
		HashCore(bytes);
		return GetValueFinal();
	}

	public byte[] ComputeHash(Span<byte> bytes) {
		HashCore(bytes);
		return HashFinal();
	}

	protected abstract T GetValueFinal();
}
