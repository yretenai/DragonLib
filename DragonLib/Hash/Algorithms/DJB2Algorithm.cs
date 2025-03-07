using System.Numerics;

namespace DragonLib.Hash.Algorithms;

// https://theartincode.stanis.me/008-djb2/
// ReSharper disable UnusedMethodReturnValue.Global
public sealed class DJB2Algorithm<T> : SpanHashAlgorithm<T>
	where T : unmanaged, INumber<T>, IBinaryInteger<T> {
	private readonly T Basis;

	public DJB2Algorithm(T basis) {
		Basis = basis;
		Reset(Basis);
	}

	public T HashNext(T value) {
		Value = (Value << 5) + Value + value;
		return Value;
	}

	protected override void HashCore(byte[] array, int ibStart, int cbSize) {
		while (cbSize > 0) {
			HashNext(T.CreateSaturating(array[ibStart++]));
			cbSize--;
		}
	}

	public void HashCore(ushort[] array, int ibStart, int cbSize) {
		while (cbSize > 0) {
			HashNext(T.CreateSaturating(array[ibStart++]));
			cbSize--;
		}
	}

	public void HashCore(uint[] array, int ibStart, int cbSize) {
		while (cbSize > 0) {
			HashNext(T.CreateSaturating(array[ibStart++]));
			cbSize--;
		}
	}

	public void HashCore(ulong[] array, int ibStart, int cbSize) {
		while (cbSize > 0) {
			HashNext(T.CreateSaturating(array[ibStart++]));
			cbSize--;
		}
	}

	public void Reset(T value) => Value = value;
	public override void Reset() => Value = Basis;
	public override void Initialize() => Reset(Basis);

	protected override T GetValueFinal() {
		var val = Value;
		Reset();
		return val;
	}
}
