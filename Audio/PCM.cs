using System;
using System.Linq;
using System.Runtime.InteropServices;
using DragonLib.Audio.WAV;
using JetBrains.Annotations;

namespace DragonLib.Audio
{
    [PublicAPI]
    public static class PCM
    {
        public static Memory<short> Merge(params Memory<short>[] channels)
        {
            switch (channels.Length)
            {
                case 0:
                    return Memory<short>.Empty;
                case 1:
                    return channels[0];
            }

            var samples = channels[0].Length;
            if (channels.Any(x => x.Length != samples)) return Memory<short>.Empty;
            var channelCount = channels.Length;
            var stream = new Memory<short>(new short[samples * channelCount]);
            for (var i = 0; i < samples; ++i)
            for (var j = 0; j < channelCount; ++j)
                stream.Span[i * channelCount + j] = channels[j].Span[i];

            return stream;
        }

        public static Memory<short>[] Separate(Memory<short> stream, int channelCount)
        {
            switch (channelCount)
            {
                case 0:
                    return new[] { Memory<short>.Empty };
                case 1:
                    return new[] { stream };
            }

            var samples = stream.Length / channelCount;
            var channels = new Memory<short>[channelCount];
            for (var i = 0; i < channelCount; ++i) channels[i] = new Memory<short>(new short[samples]);

            for (var i = 0; i < samples; ++i)
            for (var j = 0; j < channelCount; ++j)
                channels[j].Span[i] = stream.Span[i * channelCount + j];

            return channels;
        }

        public static Memory<byte> ConstructWAVE(short codec, short channels, int sampleRate, short blockAlign, int bps, Span<byte> stream)
        {
            var buffer = new Memory<byte>(new byte[44 + stream.Length]);
            var header = new WAVEHeader
            {
                Magic = 0x46464952,
                Size = buffer.Length,
                Format = 0x45564157
            };

            var fmt = new WAVEFormat
            {
                Magic = 0x20746D66,
                Size = 16,
                Format = codec,
                Channels = channels,
                SampleRate = sampleRate,
                BlockAlign = blockAlign,
                BitsPerSample = (short) bps
            };
            fmt.ByteRate = fmt.SampleRate * fmt.Channels * fmt.BitsPerSample / 8;
            var dat = new WAVEData
            {
                Magic = 0x61746164,
                Size = buffer.Length - 44
            };

            MemoryMarshal.Write(buffer.Span, ref header);
            MemoryMarshal.Write(buffer.Span.Slice(12), ref fmt);
            MemoryMarshal.Write(buffer.Span.Slice(36), ref dat);

            stream.CopyTo(buffer.Span.Slice(44));
            return buffer;
        }
    }
}
