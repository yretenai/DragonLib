using DragonLib.Hash.Basis;
using DragonLib.Hash.Generic;

namespace DragonLib.Hash;

public static class FowlerNollVo {
    public static FNVAlgorithm<uint> Create(FNV32Basis basis = FNV32Basis.Default, uint prime = 0x01000193U) => new((uint) basis, prime);
    public static FNVAlternateAlgorithm<uint> CreateAlternate(FNV32Basis basis = FNV32Basis.Default, uint prime = 0x01000193U) => new((uint) basis, prime);
    public static FNVAlgorithm<ulong> Create(FNV64Basis basis = FNV64Basis.Default, ulong prime = 0x00000100000001B3UL) => new((ulong) basis, prime);
    public static FNVAlternateAlgorithm<ulong> CreateAlternate(FNV64Basis basis = FNV64Basis.Default, ulong prime = 0x00000100000001B3UL) => new((ulong) basis, prime);
}
