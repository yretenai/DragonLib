using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonLib.Hash.Generic;

public class CRCAlgorithm<T> : SpanHashAlgorithm<T>
    where T : unmanaged, IConvertible, INumber<T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, T>, IMinMaxValue<T> {
    private readonly T Init;
    private readonly T Polynomial;
    private readonly bool ReflectIn;
    private readonly bool ReflectOut;
    private readonly T[] Table;
    private readonly T Xor;

    public unsafe CRCAlgorithm(T polynomial, T init, T xor, bool reflectIn, bool reflectOut) {
        Polynomial = polynomial;
        HashSizeValue = sizeof(T) * 8;
        Init = init;
        Xor = xor;
        ReflectIn = reflectIn;
        ReflectOut = reflectOut;
        Table = new T[256];

        Reset();
        CreateTable();
    }

#if RELEASE
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    private unsafe void CreateTable() {
        var width = sizeof(T) * 8;
        var msb = 1ul << (width - 1);
        var align = width - 8;
        var poly = Polynomial.ToUInt64(null);
        var mask = T.MaxValue.ToUInt64(null);

        for (var i = 0ul; i < 256; ++i) {
            var r = i;

            if (ReflectIn) {
                r = Reflect(r, width);
            } else if (width > 8) {
                r <<= align;
            }

            for (var j = 0; j < 8; j++) {
                if ((r & msb) != 0) {
                    r = (r << 1) ^ poly;
                } else {
                    r <<= 1;
                }
            }

            if (ReflectIn) {
                r = Reflect(r, width);
            }

            Table[i] = T.Create(r & mask);
        }
    }

#if RELEASE
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    protected override unsafe void HashCore(byte[] array, int ibStart, int cbSize) {
        while (cbSize > 0) {
            var @byte = T.Create(array[ibStart++]);
            if (ReflectOut) {
                Value = Table[(Value ^ @byte).ToUInt64(null) & 0xFF] ^ (Value >> 8);
            } else {
                Value = Table[((Value >> (sizeof(T) * 8 - 8)) ^ @byte).ToUInt64(null) & 0xFF] ^ (Value << 8);
            }

            cbSize--;
        }
    }

    protected override byte[] HashFinal() {
        Span<T> tmp = stackalloc T[1];
        tmp[0] = GetValueFinal();
        return MemoryMarshal.AsBytes(tmp).ToArray();
    }

    protected override T GetValueFinal() => Value ^ Xor;

#if RELEASE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public void Reset() => Value = Init;
    public override void Initialize() => Reset();

#if RELEASE
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    public static ulong Reflect(ulong v, int width) {
        v = ((v >> 1) & 0x5555555555555555) | ((v & 0x5555555555555555) << 1);
        v = ((v >> 2) & 0x3333333333333333) | ((v & 0x3333333333333333) << 2);
        v = ((v >> 4) & 0x0F0F0F0F0F0F0F0F) | ((v & 0x0F0F0F0F0F0F0F0F) << 4);
        v = ((v >> 8) & 0x00FF00FF00FF00FF) | ((v & 0x00FF00FF00FF00FF) << 8);
        v = ((v >> 16) & 0x0000FFFF0000FFFF) | ((v & 0x0000FFFF0000FFFF) << 16);
        v = (v >> 32) | (v << 32);
        v >>= 64 - width;
        return v;
    }
}
