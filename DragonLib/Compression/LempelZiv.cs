namespace DragonLib.Compression;

public static class LempelZiv {
    // https://github.com/Thealexbarney/LibHac/blob/e0b482f44b5f225cfe29af0b80186bcb8895806c/src/LibHac/Util/Lz4.cs
    // modifications: exit on overflow on either cmp or dec, return pos cursors.
    public static (int CmpPos, int DecPos) DecompressLZ4(ReadOnlySpan<byte> cmp, Span<byte> dec) {
        var cmpPos = 0;
        var decPos = 0;

        int GetLength(int length, ReadOnlySpan<byte> slc) {
            if (length == 0xf) {
                byte sum;
                do {
                    length += sum = slc[cmpPos++];
                } while (sum == 0xff);
            }

            return length;
        }

        do {
            var token = cmp[cmpPos++];

            var encCount = (token >> 0) & 0xf;
            var litCount = (token >> 4) & 0xf;

            litCount = GetLength(litCount, cmp);

            if (decPos + litCount > dec.Length) {
                litCount = dec.Length - decPos;
            }

            if (cmpPos + litCount > cmp.Length) {
                litCount = cmp.Length - cmpPos;
            }

            if (litCount < 0) {
                return (-1, -1);
            }

            cmp.Slice(cmpPos, litCount).CopyTo(dec[decPos..]);

            cmpPos += litCount;
            decPos += litCount;

            if (cmpPos >= cmp.Length || decPos >= dec.Length) {
                break;
            }

            var back = (cmp[cmpPos++] << 0) |
                       (cmp[cmpPos++] << 8);

            encCount = GetLength(encCount, cmp) + 4;

            var encPos = decPos - back;

            if (encCount <= back) {
                if (decPos + encCount > dec.Length || encPos + encCount > dec.Length) {
                    encCount = dec.Length - decPos;
                }

                if (encCount < 0) {
                    return (-1, -1);
                }

                dec.Slice(encPos, encCount).CopyTo(dec[decPos..]);

                decPos += encCount;

                if (decPos >= dec.Length) {
                    break;
                }
            } else {
                while (encCount-- > 0) {
                    dec[decPos++] = dec[encPos++];
                }
            }
        } while (cmpPos < cmp.Length &&
                 decPos < dec.Length);

        return (cmpPos, decPos);
    }

    // some old code, idk where it's from.
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
