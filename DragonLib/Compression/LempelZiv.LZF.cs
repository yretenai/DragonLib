namespace DragonLib.Compression;

public static partial class LempelZiv {
    // Some old code, idk where it's from. If you are the author and want to be credited, please contact me.
    public static (int CmpPos, int DecPos) DecompressLZF(ReadOnlySpan<byte> cmp, Span<byte> dec) {
        var cmpPos = 0;
        var decPos = 0;

        while (cmpPos < cmp.Length && decPos < dec.Length) {
            var token = cmp[cmpPos++];
            if (token <= 0x1F) {
                for (int i = token; i >= 0; --i) {
                    dec[decPos] = cmp[cmpPos];
                    ++decPos;
                    ++cmpPos;

                    if (cmpPos >= cmp.Length || decPos >= dec.Length) {
                        break;
                    }
                }
            } else {
                var encLen = token >> 5;
                if (encLen == 7) {
                    encLen += cmp[cmpPos++];
                }

                var dictDist = ((token & 0x1f) << 8) | cmp[cmpPos];
                ++cmpPos;
                encLen += 2;

                var encPos = decPos - 1 - dictDist;

                if (encPos + encLen > dec.Length || decPos + encLen > dec.Length) {
                    encLen = dec.Length - encPos;
                }

                for (var i = 0; i < encLen; ++i) {
                    dec[decPos] = dec[encPos + i];
                    ++decPos;
                }
            }
        }

        return (cmpPos, decPos);
    }
}
