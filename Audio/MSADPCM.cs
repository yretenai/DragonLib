using System;
using JetBrains.Annotations;

namespace DragonLib.Audio
{
    // Based off MonoGame
    // https://github.com/MonoGame/MonoGame/blob/8d46b78c7003ae273825daa3a5aad30bd3c576f0/MonoGame.Framework/Platform/Audio/AudioLoader.cs
    [PublicAPI]
    public static class MSADPCM
    {
        private static int[] StepTable = { 230, 230, 230, 230, 307, 409, 512, 614, 768, 614, 512, 409, 307, 230, 230, 230 };

        private static int[] CoefficientTable1 = { 256, 512, 0, 192, 240, 460, 392 };

        private static int[] CoefficientTable2 = { 0, -256, 0, 64, 0, -208, -232 };

        private static int MSADPCMExpandNibble(ref MSADPCMState channel, int nibble)
        {
            var nibbleSign = nibble - (((nibble & 0x08) != 0) ? 0x10 : 0);
            var predictor = ((channel.Sample1 * channel.Coefficient1) + (channel.Sample2 * channel.Coefficient2)) / 256 + (nibbleSign * channel.Delta);

            if (predictor < -32768)
                predictor = -32768;
            else if (predictor > 32767)
                predictor = 32767;

            channel.Sample2 = channel.Sample1;
            channel.Sample1 = predictor;

            channel.Delta = (StepTable[nibble] * channel.Delta) / 256;
            if (channel.Delta < 16)
                channel.Delta = 16;

            return predictor;
        }

        // Convert buffer containing MS-ADPCM wav data to a 16-bit signed PCM buffer
        public static Span<byte> Decode(Span<byte> buffer, int blockAlignment)
        {
            var state = new MSADPCMState();

            var sampleCountFullBlock = (blockAlignment - 7) * 2 + 2;
            var sampleCountLastBlock = 0;
            var count = buffer.Length;
            if ((count % blockAlignment) > 0)
                sampleCountLastBlock = (count % blockAlignment - 7) * 2 + 2;
            var sampleCount = ((count / blockAlignment) * sampleCountFullBlock) + sampleCountLastBlock;
            var samples = new byte[sampleCount * sizeof(short)];
            var sampleOffset = 0;
            var offset = 0;

            while (count > 0)
            {
                var blockSize = blockAlignment;
                if (count < blockSize)
                    blockSize = count;
                count -= blockAlignment;

                var totalSamples = (blockSize - 7) * 2 + 2;
                if (totalSamples < 2)
                    break;

                var offsetStart = offset;
                int blockPredictor = buffer[offset];
                ++offset;
                if (blockPredictor > 6)
                    blockPredictor = 6;
                state.Coefficient1 = CoefficientTable1[blockPredictor];
                state.Coefficient2 = CoefficientTable2[blockPredictor];

                state.Delta = buffer[offset];
                state.Delta |= buffer[offset + 1] << 8;
                if ((state.Delta & 0x8000) != 0)
                    state.Delta -= 0x10000;
                offset += 2;

                state.Sample1 = buffer[offset];
                state.Sample1 |= buffer[offset + 1] << 8;
                if ((state.Sample1 & 0x8000) != 0)
                    state.Sample1 -= 0x10000;
                offset += 2;

                state.Sample2 = buffer[offset];
                state.Sample2 |= buffer[offset + 1] << 8;
                if ((state.Sample2 & 0x8000) != 0)
                    state.Sample2 -= 0x10000;
                offset += 2;

                samples[sampleOffset] = (byte) state.Sample2;
                samples[sampleOffset + 1] = (byte) (state.Sample2 >> 8);
                samples[sampleOffset + 2] = (byte) state.Sample1;
                samples[sampleOffset + 3] = (byte) (state.Sample1 >> 8);
                sampleOffset += 4;

                blockSize -= (offset - offsetStart);
                for (var i = 0; i < blockSize; ++i)
                {
                    byte nibbles = buffer[offset];

                    var sample = MSADPCMExpandNibble(ref state, nibbles.GetHighNibble());
                    samples[sampleOffset] = (byte) sample;
                    samples[sampleOffset + 1] = (byte) (sample >> 8);

                    sample = MSADPCMExpandNibble(ref state, nibbles.GetLowNibble());
                    samples[sampleOffset + 2] = (byte) sample;
                    samples[sampleOffset + 3] = (byte) (sample >> 8);

                    sampleOffset += 4;
                    ++offset;
                }
            }

            return samples;
        }

        private struct MSADPCMState
        {
            public int Delta;
            public int Sample1;
            public int Sample2;
            public int Coefficient1;
            public int Coefficient2;
        }
    }
}
