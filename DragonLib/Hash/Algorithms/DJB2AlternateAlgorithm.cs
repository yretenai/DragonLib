using System.Numerics;
using System.Runtime.CompilerServices;

namespace DragonLib.Hash.Algorithms;

// https://theartincode.stanis.me/008-djb2/ with the java bug.
public sealed class DJB2AlternateAlgorithm<T> : SpanHashAlgorithm<T>
    where T : unmanaged, INumber<T>, IBinaryInteger<T> {
    private readonly T Basis;

    public DJB2AlternateAlgorithm(T basis) {
        Basis = basis;
        Reset(Basis);
    }

    public T HashNext(T value) {
        Value = (Value << 5) - Value + value;
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
    public override void Reset() => Value = Basis;

    public override void Initialize() => Reset(Basis);

    protected override T GetValueFinal() {
        var val = Value;
        Reset();
        return val;
    }
}
