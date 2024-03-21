using System.Numerics;

namespace DragonLib.Hash;

public static class ModularInverse {
	public static T Calculate<T>(T value) where T : IUnsignedNumber<T>, IMultiplyOperators<T, T, T>, IModulusOperators<T, T, T>, IShiftOperators<T, T, T> {
		var size = System.Runtime.CompilerServices.Unsafe.SizeOf<T>() * 8;
		var mod = 1 << size;
		var uValue = ulong.CreateTruncating(value);
		var modpow = BigInteger.ModPow(uValue, -1, mod);
		return T.CreateTruncating(modpow);
	}
}
