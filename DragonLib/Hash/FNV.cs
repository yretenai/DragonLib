using DragonLib.Hash.Basis;
using DragonLib.Hash.Algorithms;

namespace DragonLib.Hash;

public static class FNV {
    public const uint FNV32Prime = 0x01000193U;
    public const ulong FNV64Prime = 0x00000100000001B3UL;

    public static FNVAlgorithm<uint> Create(FNV32Basis basis = FNV32Basis.Default, uint prime = FNV32Prime) => new((uint) basis, prime);
    public static FNVInverseAlgorithm<uint> CreateInverse(FNV32Basis basis = FNV32Basis.Default, uint prime = FNV32Prime) => new((uint) basis, prime);
    public static FNVAlgorithm<ulong> Create(FNV64Basis basis = FNV64Basis.Default, ulong prime = FNV64Prime) => new((ulong) basis, prime);
    public static FNVInverseAlgorithm<ulong> CreateInverse(FNV64Basis basis = FNV64Basis.Default, ulong prime = FNV64Prime) => new((ulong) basis, prime);
}
