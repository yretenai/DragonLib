using System;
using System.Runtime.InteropServices;

namespace DragonLib.Audio {
    // Based off VGAudio
    // https://github.com/Thealexbarney/VGAudio/blob/d66b7b7b9fde1ed3b01683e0e66fff747bbd816f/src/VGAudio/Codecs/GcAdpcm/GcAdpcmDecoder.cs
    public static class GCADPCM {
        public static Span<byte> Decode(Span<byte> adpcm, short[] coefficients, int sampleCount) {
            var pcm = new short[sampleCount];

            if (sampleCount == 0) return Span<byte>.Empty;

            var frameCount = sampleCount.DivideByRoundUp(14);
            var currentSample = 0;
            var outIndex = 0;
            var inIndex = 0;
            short hist1 = 0;
            short hist2 = 0;

            for (var i = 0; i < frameCount; i++) {
                if (inIndex >= adpcm.Length) break;

                var predictorScale = adpcm[inIndex++];
                var scale = (1 << predictorScale.GetLowNibble()) * 2048;
                int predictor = predictorScale.GetHighNibble();
                var coef1 = coefficients[predictor * 2];
                var coef2 = coefficients[predictor * 2 + 1];

                var samplesToRead = Math.Min(14, sampleCount - currentSample);

                for (var s = 0; s < samplesToRead; s++) {
                    if (inIndex >= adpcm.Length) break;

                    int adpcmSample = s % 2 == 0 ? adpcm[inIndex].GetHighNibbleSigned() : adpcm[inIndex++].GetLowNibbleSigned();
                    var distance = scale * adpcmSample;
                    var predictedSample = coef1 * hist1 + coef2 * hist2;
                    var correctedSample = predictedSample + distance;
                    var scaledSample = (correctedSample + 1024) >> 11;
                    var clampedSample = scaledSample.ShortClamp();

                    hist2 = hist1;
                    hist1 = clampedSample;

                    pcm[outIndex++] = clampedSample;
                    currentSample++;
                }
            }

            return MemoryMarshal.Cast<short, byte>(pcm);
        }
    }
}
