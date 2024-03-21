using DragonLib.Hash.Algorithms;

namespace DragonLib.Hash;

public static class DJB2 {
	public const uint Basis = 0x1505;
	public static DJB2Algorithm<uint> Create(uint basis = Basis) => new(basis);
	public static DJB2AlternateAlgorithm<uint> CreateAlternate(uint basis = Basis) => new(basis);
}
