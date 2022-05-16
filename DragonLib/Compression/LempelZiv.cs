namespace DragonLib.Compression;

public static class LempelZiv {
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
}
