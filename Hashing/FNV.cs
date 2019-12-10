using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace DragonLib.Hashing
{
    [PublicAPI]
    public static class FNV
    {
        public const ulong FNV1_BASIS_64_BIT = 0xCBF29CE484222645UL;
        public const ulong FNV1_PRIME_64_BIT = 0x00000100000001B3UL;
        public const uint FNV1_BASIS_32_BIT = 0x811C9DC5U;
        public const uint FNV1_PRIME_32_BIT = 0x01000193U;

        public static ulong FNV1UI64<T>(Span<T> data, ulong basis = FNV1_BASIS_64_BIT, ulong prime = FNV1_PRIME_64_BIT) where T : struct
        {
            var byteData = MemoryMarshal.Cast<T, byte>(data);
            var hash = basis;
            foreach (var b in byteData)
            {
                hash *= prime;
                hash ^= b;
            }

            return hash;
        }

        public static uint FNV1UI32<T>(Span<T> data, uint basis = FNV1_BASIS_32_BIT, uint prime = FNV1_PRIME_32_BIT) where T : struct
        {
            var byteData = MemoryMarshal.Cast<T, byte>(data);
            var hash = basis;
            foreach (var b in byteData)
            {
                hash *= prime;
                hash ^= b;
            }

            return hash;
        }

        public static ulong FNV1aUI64<T>(Span<T> data, ulong basis = FNV1_BASIS_64_BIT, ulong prime = FNV1_PRIME_64_BIT) where T : struct
        {
            var byteData = MemoryMarshal.Cast<T, byte>(data);
            var hash = basis;
            foreach (var b in byteData)
            {
                hash ^= b;
                hash *= prime;
            }

            return hash;
        }

        public static uint FNV1aUI32<T>(Span<T> data, uint basis = FNV1_BASIS_32_BIT, uint prime = FNV1_PRIME_32_BIT) where T : struct
        {
            var byteData = MemoryMarshal.Cast<T, byte>(data);
            var hash = basis;
            foreach (var b in byteData)
            {
                hash ^= b;
                hash *= prime;
            }

            return hash;
        }
    }
}
