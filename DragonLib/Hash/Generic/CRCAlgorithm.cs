using System.Runtime.InteropServices;

namespace DragonLib.Hash.Generic;

public class CRCAlgorithm<T> : SpanHashAlgorithm<T>
    where T : unmanaged, IConvertible, INumber<T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, T>, IMinMaxValue<T> {
    private readonly T Polynomial;
    private readonly T Seed;
    private readonly T[] Table;

    public unsafe CRCAlgorithm(T polynomial, T seed) {
        Polynomial = polynomial;
        HashSizeValue = sizeof(T) * 8;
        Seed = seed;
        Table = new T[256];

        CreateTable();
    }

    private void CreateTable() {
        var one = T.Create(1);
        for (var i = 0; i < 256; ++i) {
            var value = T.Create(i);

            for (var j = 0; j < 8; ++j) {
                if ((value & one) == one) {
                    value = (value >> 1) ^ Polynomial;
                } else {
                    value >>= 1;
                }
            }

            Table[i] = value;
        }
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize) {
        while (cbSize > 0) {
            Value = Table[((Value ^ T.Create(array[ibStart++])) & T.Create(0xFF)).ToByte(null)] ^ (Value >> 8);
            cbSize--;
        }
    }

    protected override byte[] HashFinal() {
        Span<T> tmp = stackalloc T[1];
        tmp[0] = Value;
        return MemoryMarshal.AsBytes(tmp).ToArray();
    }

    public override void Initialize() => Value = Seed;

    public static T Reflect(T value) {
        var reflected = T.Create(0);
        var one = T.Create(1);
        unsafe {
            for (var i = 0; i < sizeof(T) * 8; i++) {
                reflected <<= 1;
                if ((value & one) == one) {
                    reflected |= one;
                }

                value >>= 1;
            }
        }

        return reflected;
    }
}
