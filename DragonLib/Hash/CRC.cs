using DragonLib.Hash.Algorithms;
using DragonLib.Hash.Basis;

namespace DragonLib.Hash;

public static class CRC {
	public static CRCAlgorithm<ulong> Create(CRCVariant<ulong> variant) => new(variant.Polynomial, variant.Init, variant.Xor, variant.ReflectIn, variant.ReflectOut);
	public static CRCAlgorithm<uint> Create(CRCVariant<uint> variant) => new(variant.Polynomial, variant.Init, variant.Xor, variant.ReflectIn, variant.ReflectOut);
	public static CRCAlgorithm<ushort> Create(CRCVariant<ushort> variant) => new(variant.Polynomial, variant.Init, variant.Xor, variant.ReflectIn, variant.ReflectOut);
	public static CRCAlgorithm<byte> Create(CRCVariant<byte> variant) => new(variant.Polynomial, variant.Init, variant.Xor, variant.ReflectIn, variant.ReflectOut);
}
