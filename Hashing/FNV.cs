using System;
using System.Runtime.InteropServices;

namespace DragonLib.Hashing {
    public static class FNV {
        public const ulong FNV1Basis64Bit = 0xCBF29CE484222645UL;
        public const ulong FNV1Prime64Bit = 0x00000100000001B3UL;
        public const uint FNV1Basis32Bit = 0x811C9DC5U;
        public const uint FNV1Prime32Bit = 0x01000193U;

        public static ulong FNV1UI64<T>(Span<T> data, ulong basis = FNV1Basis64Bit, ulong prime = FNV1Prime64Bit) where T : struct {
            var byteData = MemoryMarshal.AsBytes(data);
            var hash = basis;
            foreach (var b in byteData) {
                hash *= prime;
                hash ^= b;
            }

            return hash;
        }

        public static uint FNV1UI32<T>(Span<T> data, uint basis = FNV1Basis32Bit, uint prime = FNV1Prime32Bit) where T : struct {
            var byteData = MemoryMarshal.AsBytes(data);
            var hash = basis;
            foreach (var b in byteData) {
                hash *= prime;
                hash ^= b;
            }

            return hash;
        }

        // ReSharper disable once InconsistentNaming
        public static ulong FNV1aUI64<T>(Span<T> data, ulong basis = FNV1Basis64Bit, ulong prime = FNV1Prime64Bit) where T : struct {
            var byteData = MemoryMarshal.AsBytes(data);
            var hash = basis;
            foreach (var b in byteData) {
                hash ^= b;
                hash *= prime;
            }

            return hash;
        }

        // ReSharper disable once InconsistentNaming
        public static uint FNV1aUI32<T>(Span<T> data, uint basis = FNV1Basis32Bit, uint prime = FNV1Prime32Bit) where T : struct {
            var byteData = MemoryMarshal.AsBytes(data);
            var hash = basis;
            foreach (var b in byteData) {
                hash ^= b;
                hash *= prime;
            }

            return hash;
        }
    }
}
