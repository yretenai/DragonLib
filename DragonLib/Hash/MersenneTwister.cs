using DragonLib.Hash.Generic;

namespace DragonLib.Hash;

public static class MersenneTwister {
    public static MTRNGAlgorithm<ulong> Create(ulong seed) => new(seed, 312, 156, 31, 0xB5026F5AA96619E9, 0x5555555555555555, 0x71D67FFFEDA60000, 0xFFF7EEE000000000, 29, 17, 37, 43, 0x5851F42D4C957F2D);
    public static MTRNGAlgorithm<uint> Create(uint seed) => new(seed, 624, 397, 31, 0x9908B0DF, 0xFFFFFFFF, 0x9D2C5680, 0xEFC60000, 11, 7, 15, 18, 0x6C078965);
}
