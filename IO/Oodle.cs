using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DragonLib.IO {
    public static class Oodle {
        private static OodleLZ_Decompress? DecompressDelegate { get; set; }

        public static unsafe Memory<byte> Decompress(ReadOnlySpan<byte> input, int size) {
            if (DecompressDelegate == null) return Memory<byte>.Empty;

            if (input.Length < 2) return input.ToArray();

            Memory<byte> buffer = new byte[size];
            fixed (void* src = &input.GetPinnableReference()) {
                using var pinned = buffer.Pin();

                var outSize = DecompressDelegate.Invoke(src, input.Length, pinned.Pointer, buffer.Length, 0, 0, 0, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, 3);
                return buffer[..outSize];
            }
        }

        public static void Load(string? path) {
            if (path?.EndsWith(".dll") == false) path = Directory.EnumerateFiles(path, "oo2core_*_win64.dll").FirstOrDefault();

            if (path == null) return;

            var handle = NativeLibrary.Load(path);
            var address = NativeLibrary.GetExport(handle, "OodleLZ_Decompress");
            DecompressDelegate = Marshal.GetDelegateForFunctionPointer<OodleLZ_Decompress>(address);
        }

        private unsafe delegate int OodleLZ_Decompress(void* srcBuf, int srcSize, void* dstBuf, int dstSize, int fuzz, int crc, int verbose, nint dstBase, int e, nint cb, nint cbContext, nint scratch, uint scratchSize, uint threadPhase);
    }
}
