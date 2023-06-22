using DragonLib.Hash.Algorithms;

namespace DragonLib.Hash;

public static class DJB2 {
    public static DJB2Algorithm<uint> Create(uint basis = 5381) => new(basis);
    public static DJB2AlternateAlgorithm<uint> CreateAlternate(uint basis = 5381) => new(basis);
}
