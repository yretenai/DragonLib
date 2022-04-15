using DragonLib.Hash.Basis;
using DragonLib.Hash.Generic;

namespace DragonLib.Hash;

public static class CyclicRedundancyCheck {
    public static CRCAlgorithm<ulong> Create(CRC64Polynomial polynomial = CRC64Polynomial.Default, ulong seed = ulong.MaxValue) => new((ulong) polynomial, seed);
    public static CRCAlgorithm<uint> Create(CRC32Polynomial polynomial = CRC32Polynomial.Default, uint seed = uint.MaxValue) => new((uint) polynomial, seed);
    public static CRCAlgorithm<ushort> Create(CRC16Polynomial polynomial, ushort seed = ushort.MaxValue) => new((ushort) polynomial, seed);
    public static CRCAlgorithm<byte> Create(CRC8Polynomial polynomial, byte seed = byte.MaxValue) => new((byte) polynomial, seed);
}
