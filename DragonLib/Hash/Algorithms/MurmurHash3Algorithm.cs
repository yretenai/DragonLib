using System.Numerics;
using System.Runtime.InteropServices;

namespace DragonLib.Hash.Algorithms;

public static class MurmurHash3Algorithm {
	public static uint FMix32(uint h) {
		h ^= h >> 16;
		h *= 0x85ebca6bu;
		h ^= h >> 13;
		h *= 0xc2b2ae35u;
		h ^= h >> 16;
		return h;
	}

	public static ulong FMix64(ulong h) {
		h ^= h >> 33;
		h *= 0xff51afd7ed558ccdul;
		h ^= h >> 33;
		h *= 0xc4ceb9fe1a85ec53ul;
		h ^= h >> 33;
		return h;
	}

	public static uint Hash32_32(ReadOnlySpan<byte> key, uint seed = 0,
		uint c1 = 0xcc9e2d51, uint c2 = 0x1b873593,
		uint e = 0xe6546b64) {
		var h1 = seed;
		uint k1;

		var blocks = MemoryMarshal.Cast<byte, uint>(key);
		foreach (var block in blocks) {
			k1 = block * c1;
			k1 = BitOperations.RotateLeft(k1, 15);
			k1 *= c2;

			h1 ^= k1;
			h1 = BitOperations.RotateLeft(h1, 13);
			h1 = h1 * 5 + e;
		}

		k1 = 0u;

		var tail = key[(blocks.Length << 2)..];
		switch (key.Length) {
			case 3:
				k1 ^= (uint) tail[2] << 16;
				goto case 2;
			case 2:
				k1 ^= (uint) tail[1] << 8;
				goto case 1;
			case 1:
				k1 ^= tail[0];
				k1 *= c1;
				k1 = BitOperations.RotateLeft(k1, 15);
				k1 *= c2;
				h1 ^= k1;
				break;
		}

		return FMix32(h1 ^ (uint) key.Length);
	}

	public static (uint, uint, uint, uint) Hash32_128(ReadOnlySpan<byte> key, uint seed = 0,
		uint c1 = 0x239b961b, uint c2 = 0xab0e9789, uint c3 = 0x38b34ae5, uint c4 = 0xa8b34ae5,
		uint e1 = 0x561ccd1b, uint e2 = 0x0bcaa747, uint e3 = 0x96cd1c35, uint e4 = 0x32ac3b17) {
		var h1 = seed;
		var h2 = seed;
		var h3 = seed;
		var h4 = seed;
		uint k1;
		uint k2;
		uint k3;
		uint k4;

		var blocks = MemoryMarshal.Cast<byte, uint>(key);
		for (var index = 0; index < blocks.Length; index += 4) {
			k1 = blocks[index];
			k2 = blocks[index + 1];
			k3 = blocks[index + 2];
			k4 = blocks[index + 3];

			k1 *= c1;
			k1 = BitOperations.RotateLeft(k1, 15);
			k1 *= c2;
			h1 ^= k1;

			h1 = BitOperations.RotateLeft(h1, 19);
			h1 += h2;
			h1 = h1 * 5 + e1;

			k2 *= c2;
			k2 = BitOperations.RotateLeft(k2, 16);
			k2 *= c3;
			h2 ^= k2;

			h2 = BitOperations.RotateLeft(h2, 17);
			h2 += h3;
			h2 = h2 * 5 + e2;

			k3 *= c3;
			k3 = BitOperations.RotateLeft(k3, 17);
			k3 *= c4;
			h3 ^= k3;

			h3 = BitOperations.RotateLeft(h3, 15);
			h3 += h4;
			h3 = h3 * 5 + e3;

			k4 *= c4;
			k4 = BitOperations.RotateLeft(k4, 18);
			k4 *= c1;
			h4 ^= k4;

			h4 = BitOperations.RotateLeft(h4, 13);
			h4 += h1;
			h4 = h4 * 5 + e4;
		}

		k1 = 0u;
		k2 = 0u;
		k3 = 0u;
		k4 = 0u;

		var tail = key[(blocks.Length << 4)..];
		switch (key.Length) {
			case 15:
				k4 ^= (uint) tail[14] << 16;
				goto case 15;
			case 14:
				k4 ^= (uint) tail[13] << 8;
				goto case 14;
			case 13:
				k4 ^= (uint) tail[12] << 0;
				k4 *= c4;
				k4 = BitOperations.RotateLeft(k4, 18);
				k4 *= c1;
				h4 ^= k4;
				goto case 12;

			case 12:
				k3 ^= (uint) tail[11] << 24;
				goto case 11;
			case 11:
				k3 ^= (uint) tail[10] << 16;
				goto case 10;
			case 10:
				k3 ^= (uint) tail[9] << 8;
				goto case 9;
			case 9:
				k3 ^= (uint) tail[8] << 0;
				k3 *= c3;
				k3 = BitOperations.RotateLeft(k3, 17);
				k3 *= c4;
				h3 ^= k3;
				goto case 8;

			case 8:
				k2 ^= (uint) tail[7] << 24;
				goto case 7;
			case 7:
				k2 ^= (uint) tail[6] << 16;
				goto case 6;
			case 6:
				k2 ^= (uint) tail[5] << 8;
				goto case 5;
			case 5:
				k2 ^= (uint) tail[4] << 0;
				k2 *= c2;
				k2 = BitOperations.RotateLeft(k2, 16);
				k2 *= c3;
				h2 ^= k2;
				goto case 4;

			case 4:
				k1 ^= (uint) tail[3] << 24;
				goto case 3;
			case 3:
				k1 ^= (uint) tail[2] << 16;
				goto case 2;
			case 2:
				k1 ^= (uint) tail[1] << 8;
				goto case 1;
			case 1:
				k1 ^= (uint) tail[0] << 0;
				k1 *= c1;
				k1 = BitOperations.RotateLeft(k1, 15);
				k1 *= c2;
				h1 ^= k1;
				break;
		}

		var len = (uint) key.Length;
		h1 ^= len;
		h2 ^= len;
		h3 ^= len;
		h4 ^= len;

		h1 += h2;
		h1 += h3;
		h1 += h4;
		h2 += h1;
		h3 += h1;
		h4 += h1;

		h1 = FMix32(h1);
		h2 = FMix32(h2);
		h3 = FMix32(h3);
		h4 = FMix32(h4);

		h1 += h2;
		h1 += h3;
		h1 += h4;
		h2 += h1;
		h3 += h1;
		h4 += h1;

		return (h1, h2, h3, h4);
	}


	public static (ulong, ulong) Hash64_128(ReadOnlySpan<byte> key, ulong seed = 0,
		ulong c1 = 0x87c37b91114253d5, ulong c2 = 0x4cf5ad432745937f,
		ulong e1 = 0x52dce729, ulong e2 = 0x38495ab5) {
		var h1 = seed;
		var h2 = seed;
		ulong k1;
		ulong k2;

		var blocks = MemoryMarshal.Cast<byte, ulong>(key);
		for (var index = 0; index < blocks.Length; index += 2) {
			k1 = blocks[index];
			k2 = blocks[index + 1];

			k1 *= c1;
			k1 = BitOperations.RotateLeft(k1, 31);
			k1 *= c2;
			h1 ^= k1;

			h1 = BitOperations.RotateLeft(h1, 27);
			h1 += h2;
			h1 = h1 * 5 + e1;

			k2 *= c2;
			k2 = BitOperations.RotateLeft(k2, 33);
			k2 *= c1;
			h2 ^= k2;

			h2 = BitOperations.RotateLeft(h2, 31);
			h2 += h1;
			h2 = h2 * 5 + e2;
		}

		k1 = 0;
		k2 = 0;

		var tail = key[(blocks.Length << 4)..];
		switch (key.Length) {
			case 15:
				k2 ^= (ulong) tail[14] << 48;
				goto case 14;
			case 14:
				k2 ^= (ulong) tail[13] << 40;
				goto case 13;
			case 13:
				k2 ^= (ulong) tail[12] << 32;
				goto case 12;
			case 12:
				k2 ^= (ulong) tail[11] << 24;
				goto case 11;
			case 11:
				k2 ^= (ulong) tail[10] << 16;
				goto case 10;
			case 10:
				k2 ^= (ulong) tail[9] << 8;
				goto case 9;
			case 9:
				k2 ^= (ulong) tail[8] << 0;
				k2 *= c2;
				k2 = BitOperations.RotateLeft(k2, 33);
				k2 *= c1;
				h2 ^= k2;
				goto case 8;

			case 8:
				k1 ^= (ulong) tail[7] << 56;
				goto case 7;
			case 7:
				k1 ^= (ulong) tail[6] << 48;
				goto case 6;
			case 6:
				k1 ^= (ulong) tail[5] << 40;
				goto case 5;
			case 5:
				k1 ^= (ulong) tail[4] << 32;
				goto case 4;
			case 4:
				k1 ^= (ulong) tail[3] << 24;
				goto case 3;
			case 3:
				k1 ^= (ulong) tail[2] << 16;
				goto case 2;
			case 2:
				k1 ^= (ulong) tail[1] << 8;
				goto case 1;
			case 1:
				k1 ^= (ulong) tail[0] << 0;
				k1 *= c1;
				k1 = BitOperations.RotateLeft(k1, 31);
				k1 *= c2;
				h1 ^= k1;
				break;
		}

		var len = (uint) key.Length;
		h1 ^= len;
		h2 ^= len;

		h1 += h2;
		h2 += h1;

		h1 = FMix64(h1);
		h2 = FMix64(h2);

		h1 += h2;
		h2 += h1;

		return (h1, h2);
	}
}
