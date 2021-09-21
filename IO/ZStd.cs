using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace DragonLib.IO {
    public static class ZStd {
        public static unsafe Memory<byte> Decompress(Memory<byte> input, long size = -1, ZStdConfiguration? configuration = null) {
            using var inputPin = input.Pin();

            configuration ??= ZStdConfiguration.Default;

            if (size <= 0 && !configuration.Magicless) size = (long)Native.ZSTD_getFrameContentSize((nint)inputPin.Pointer, (nuint)input.Length);

            switch (size) {
                case 0:
                    return Memory<byte>.Empty;
                case -1:
                    size = input.Length * 32;
                    break;
                case -2:
                    throw new Exception("Zstd reported an error while calling getFrameContentSize");
            }

            var dctx = Native.ZSTD_createDCtx();
            try {
                Native.ZSTD_DCtx_reset(dctx, 1);
                configuration.ApplyConfiguration(dctx);

                Memory<byte> buffer = new byte[size];

                using var outputPin = buffer.Pin();
                nuint result;
                if (configuration.Dict == null || configuration.Dict.Pointer == IntPtr.Zero)
                    result = Native.ZSTD_decompress(dctx, (nint)outputPin.Pointer, (nuint)size, (nint)inputPin.Pointer, (nuint)input.Length);
                else
                    result = Native.ZSTD_decompress_usingDDict(dctx, (nint)outputPin.Pointer, (nuint)size, (nint)inputPin.Pointer, (nuint)input.Length, configuration.Dict.Pointer);

                if (Native.ZSTD_isError(result) > 0) throw new Exception(Marshal.PtrToStringAnsi(Native.ZSTD_getErrorName(result)));

                return buffer;
            } finally {
                Native.ZSTD_freeDCtx(dctx);
            }
        }

        public static class Native {
            public enum ZSTD_bufferMode_e {
                ZSTD_bm_buffered = 0,
                ZSTD_bm_stable = 1
            }

            public enum ZSTD_dParameter {
                ZSTD_d_windowLogMax = 100,
                ZSTD_d_format = 1000,
                ZSTD_d_stableOutBuffer = 1001,
                ZSTD_d_forceIgnoreChecksum = 1002,
                ZSTD_d_refMultipleDDicts = 1003
            }

            public enum ZSTD_forceIgnoreChecksum_e {
                ZSTD_d_validateChecksum = 0,
                ZSTD_d_ignoreChecksum = 1
            }

            public enum ZSTD_format_e {
                ZSTD_f_zstd1 = 0,
                ZSTD_f_zstd1_magicless = 1
            }

            public enum ZSTD_refMultipleDDicts_e {
                ZSTD_rmd_refSingleDDict = 0,
                ZSTD_rmd_refMultipleDDicts = 1
            }

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nint ZSTD_createDCtx();

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZSTD_DCtx_setParameter(nint dctx, ZSTD_dParameter param, int value);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZSTD_freeDCtx(nint dctx);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZSTD_decompress(nint dctx, nint dst, nuint dstCapacity, nint src, nuint srcSize);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZSTD_decompress_usingDDict(nint dctx, nint dst, nuint dstCapacity, nint src, nuint srcSize, nint ddict);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint ZSTD_isError(nuint code);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nint ZSTD_getErrorName(nuint code);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZSTD_getFrameContentSize(nint src, nuint srcSize);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nint ZSTD_createDDict(nint dict, nuint dictSize);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZSTD_freeDDict(nint ddict);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZSTD_sizeof_DDict(nint ddict);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZDICT_getDictHeaderSize(nint dict, nuint dictSize);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint ZDICT_getDictID(nint dict, nuint dictSize);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZSTD_DStreamInSize();

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZSTD_DStreamOutSize();

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nint ZSTD_createDStream();

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZSTD_DCtx_reset(nint dctx, int directive);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZSTD_freeDStream(nint zds);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZSTD_initDStream(nint zds);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZSTD_initDStream_usingDDict(nint zds, nint ddict);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZSTD_decompressStream(nint zds, ref ZSTD_Buffer output, ref ZSTD_Buffer input);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZDICT_optimizeTrainFromBuffer_fastCover(nint dictBuffer, nuint dictBufferCapacity, nint samplesBuffer, nint samplesSizes, uint nbSamples, ref ZDICT_fastCover_params_t parameters);

            [DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
            public static extern nuint ZDICT_trainFromBuffer_fastCover(nint dictBuffer, nuint dictBufferCapacity, nint samplesBuffer, nint samplesSizes, uint nbSamples, ref ZDICT_fastCover_params_t parameters);

            [StructLayout(LayoutKind.Sequential)]
            public struct ZSTD_Buffer {
                public nint Buffer;
                public nuint Size;
                public nuint Pos;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct ZDICT_params_t {
                public int CompressionLevel;
                public uint NotificationLevel;
                public uint DictID;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct ZDICT_fastCover_params_t {
                public uint K;
                public uint D;
                public uint F;
                public uint Steps;
                public uint Threads;
                public double SplitPoint;
                public uint Accel;
                public uint ShrinkDict;
                public uint ShrinkDictMaxRegression;
                public ZDICT_params_t ZParams;
            }
        }
    }

    public sealed class ZStdStream : IDisposable {
        public ZStdStream(Memory<byte> input, ZStdConfiguration? configuration = null) {
            Data = input;
            Configuration = configuration ?? ZStdConfiguration.Default;
            InSize = ZStd.Native.ZSTD_DStreamInSize();
            OutSize = ZStd.Native.ZSTD_DStreamOutSize();
            Context = ZStd.Native.ZSTD_createDStream();
            try {
                nuint result;
                if (Configuration.Dict == null || Configuration.Dict.Pointer == IntPtr.Zero)
                    result = ZStd.Native.ZSTD_initDStream(Context);
                else
                    result = ZStd.Native.ZSTD_initDStream_usingDDict(Context, Configuration.Dict.Pointer);

                if (ZStd.Native.ZSTD_isError(result) > 0) throw new Exception(Marshal.PtrToStringAnsi(ZStd.Native.ZSTD_getErrorName(result)));

                Configuration.ApplyConfiguration(Context);
            } catch {
                ZStd.Native.ZSTD_freeDStream(Context);
                Context = IntPtr.Zero;
                throw;
            }
        }

        private Memory<byte> Data { get; }
        private ZStdConfiguration Configuration { get; }
        private nuint InSize { get; }
        private nuint OutSize { get; }
        private nuint FrameOffset { get; set; }
        private nuint Offset { get; set; }
        private nint Context { get; set; }

        public void Dispose() {
            Configuration.Dict?.Dispose();
            if (Context != IntPtr.Zero) {
                ZStd.Native.ZSTD_freeDStream(Context);
                Context = IntPtr.Zero;
            }
        }

        public unsafe Memory<byte> Next() {
            if (FrameOffset >= InSize) {
                FrameOffset = 0;
                Offset += InSize;
            }

            if (Offset >= (nuint)Data.Length) return Memory<byte>.Empty;

            var slice = Data.Slice((int)Offset, (int)Math.Min(InSize, (nuint)Data.Length - Offset));
            using var inPin = slice.Pin();
            var output = new Memory<byte>(new byte[OutSize]);
            using var outPin = output.Pin();

            var @in = new ZStd.Native.ZSTD_Buffer { Buffer = (nint)inPin.Pointer, Size = (nuint)slice.Length, Pos = FrameOffset };
            var @out = new ZStd.Native.ZSTD_Buffer { Buffer = (nint)outPin.Pointer, Size = (nuint)output.Length, Pos = 0 };
            var result = ZStd.Native.ZSTD_decompressStream(Context, ref @out, ref @in);
            if (ZStd.Native.ZSTD_isError(result) > 0) throw new Exception(Marshal.PtrToStringAnsi(ZStd.Native.ZSTD_getErrorName(result)));

            FrameOffset = @in.Pos;

            return output[..(int)@out.Pos];
        }

        public Memory<byte> End() {
            var buffer = Memory<byte>.Empty;
            while (true) {
                var block = Next();
                if (block.IsEmpty) break;

                var tmp = new Memory<byte>(new byte[buffer.Length + block.Length]);
                buffer.CopyTo(tmp);
                block.CopyTo(tmp[buffer.Length..]);
                buffer = tmp;
            }

            return buffer;
        }
    }

    public sealed class ZStdDDict : IDisposable {
        public unsafe ZStdDDict(Memory<byte> dict, nuint size = 0) {
            using var dictPin = dict.Pin();
            var result = ZStd.Native.ZDICT_getDictID((nint)dictPin.Pointer, (nuint)dict.Length);
            if (ZStd.Native.ZSTD_isError(result) > 0) throw new Exception(Marshal.PtrToStringAnsi(ZStd.Native.ZSTD_getErrorName(result)));

            Id = result;
            HeaderSize = ZStd.Native.ZDICT_getDictHeaderSize((nint)dictPin.Pointer, (nuint)dict.Length);
            if (size > 0)
                BufferSize = HeaderSize + size;
            else
                BufferSize = (nuint)dict.Length;

            Pointer = ZStd.Native.ZSTD_createDDict((nint)dictPin.Pointer, BufferSize);
            Size = ZStd.Native.ZSTD_sizeof_DDict(Pointer);
        }

        public nint Pointer { get; private set; }
        public nuint Size { get; }
        public nuint BufferSize { get; }
        public nuint HeaderSize { get; }
        public uint Id { get; }

        public void Dispose() {
            ZStd.Native.ZSTD_freeDDict(Pointer);
            Pointer = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }

        public static unsafe Memory<byte> TrainFromBuffer(long bufferSize, ZStd.Native.ZDICT_fastCover_params_t @params, bool optimize, params Memory<byte>[] samples) {
            var sampleBuffer = new Memory<byte>(new byte[samples.Sum(x => x.Length)]);
            var sampleSizes = new Memory<nuint>(new nuint[samples.Length]);
            var cur = 0;
            for (var index = 0; index < samples.Length; index++) {
                var sample = samples[index];
                sample.CopyTo(sampleBuffer[cur..]);
                cur += sample.Length;
                sampleSizes.Span[index] = (nuint)sample.Length;
            }

            Memory<byte> buffer = new byte[bufferSize];
            using var bufferPin = buffer.Pin();
            using var sampleBufferPin = sampleBuffer.Pin();
            using var sampleSizesPin = sampleSizes.Pin();
            var result = optimize ? ZStd.Native.ZDICT_optimizeTrainFromBuffer_fastCover((nint)bufferPin.Pointer, (nuint)bufferSize, (nint)sampleBufferPin.Pointer, (nint)sampleSizesPin.Pointer, (uint)samples.Length, ref @params) : ZStd.Native.ZDICT_trainFromBuffer_fastCover((nint)bufferPin.Pointer, (nuint)bufferSize, (nint)sampleBufferPin.Pointer, (nint)sampleSizesPin.Pointer, (uint)samples.Length, ref @params);
            if (ZStd.Native.ZSTD_isError(result) > 0) throw new Exception(Marshal.PtrToStringAnsi(ZStd.Native.ZSTD_getErrorName(result)));

            return buffer;
        }

        public override int GetHashCode() {
            return Pointer.GetHashCode();
        }

        public override bool Equals(object? obj) {
            return Pointer.Equals(obj);
        }

        public override string ToString() {
            return $"ZStdDDict {{ 0x{Pointer.ToString("x")} }}";
        }

        ~ZStdDDict() {
            Dispose();
        }
    }

    public record ZStdConfiguration {
        public static ZStdConfiguration Default { get; } = new();
        public ZStdDDict? Dict { get; init; }
        public bool Magicless { get; init; }
        public bool ForceIgnoreChecksum { get; init; }
        public bool StableOutBuffer { get; init; }
        public bool ReferenceMultipleDDicts { get; init; }
        public int WindowLogMax { get; init; }
        public Dictionary<int, int> OtherOptions { get; set; } = new();

        public void ApplyConfiguration(nint dctx) {
            nuint result;
            if (Magicless) {
                result = ZStd.Native.ZSTD_DCtx_setParameter(dctx, ZStd.Native.ZSTD_dParameter.ZSTD_d_format, (int)ZStd.Native.ZSTD_format_e.ZSTD_f_zstd1_magicless);
                if (ZStd.Native.ZSTD_isError(result) > 0) throw new Exception(Marshal.PtrToStringAnsi(ZStd.Native.ZSTD_getErrorName(result)));
            }

            if (ForceIgnoreChecksum) {
                result = ZStd.Native.ZSTD_DCtx_setParameter(dctx, ZStd.Native.ZSTD_dParameter.ZSTD_d_forceIgnoreChecksum, (int)ZStd.Native.ZSTD_forceIgnoreChecksum_e.ZSTD_d_ignoreChecksum);
                if (ZStd.Native.ZSTD_isError(result) > 0) throw new Exception(Marshal.PtrToStringAnsi(ZStd.Native.ZSTD_getErrorName(result)));
            }

            if (StableOutBuffer) {
                result = ZStd.Native.ZSTD_DCtx_setParameter(dctx, ZStd.Native.ZSTD_dParameter.ZSTD_d_stableOutBuffer, (int)ZStd.Native.ZSTD_bufferMode_e.ZSTD_bm_stable);
                if (ZStd.Native.ZSTD_isError(result) > 0) throw new Exception(Marshal.PtrToStringAnsi(ZStd.Native.ZSTD_getErrorName(result)));
            }

            if (ReferenceMultipleDDicts) {
                result = ZStd.Native.ZSTD_DCtx_setParameter(dctx, ZStd.Native.ZSTD_dParameter.ZSTD_d_refMultipleDDicts, (int)ZStd.Native.ZSTD_refMultipleDDicts_e.ZSTD_rmd_refMultipleDDicts);
                if (ZStd.Native.ZSTD_isError(result) > 0) throw new Exception(Marshal.PtrToStringAnsi(ZStd.Native.ZSTD_getErrorName(result)));
            }

            if (WindowLogMax > 0) {
                result = ZStd.Native.ZSTD_DCtx_setParameter(dctx, ZStd.Native.ZSTD_dParameter.ZSTD_d_windowLogMax, WindowLogMax);
                if (ZStd.Native.ZSTD_isError(result) > 0) throw new Exception(Marshal.PtrToStringAnsi(ZStd.Native.ZSTD_getErrorName(result)));
            }

            if (OtherOptions.Count > 0)
                foreach (var (parameter, value) in OtherOptions) {
                    result = ZStd.Native.ZSTD_DCtx_setParameter(dctx, (ZStd.Native.ZSTD_dParameter)parameter, value);
                    if (ZStd.Native.ZSTD_isError(result) > 0) throw new Exception(Marshal.PtrToStringAnsi(ZStd.Native.ZSTD_getErrorName(result)));
                }
        }
    }
}
