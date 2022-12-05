// original source: https://github.com/Thealexbarney/LibHac/blob/e0b482f44b5f225cfe29af0b80186bcb8895806c/src/LibHac/Util/Lz4.cs
/*
MIT License

Copyright (c) 2018 Alex Barney

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

// modifications:
//  exit on overflow on either cmp or dec
//  return pos cursors

namespace DragonLib.Compression;

public static partial class LempelZiv {
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
