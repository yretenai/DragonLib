namespace DragonLib.IO;

public static class Signature {
    public static int FindSignature(Span<byte> buffer, Span<short> signature) {
        if (signature.Length == 0) {
            return -1;
        }

        for (var ptr = 0; ptr < buffer.Length - signature.Length; ++ptr) {
            var found = true;
            for (var i = 0; i < signature.Length; ++i) {
                var b = signature[i];
                unchecked {
                    if (b != -1 && (byte) b != buffer[ptr + i]) {
                        found = false;
                        break;
                    }
                }
            }

            if (found) {
                return ptr;
            }
        }

        return -1;
    }

    public static List<int> FindSignatures(Span<byte> buffer, Span<short> signature, int stop = -1, int limit = 0) {
        if (signature.Length == 0) {
            return new List<int>(0);
        }

        if (stop == -1) {
            stop = buffer.Length - signature.Length + 1;
        }

        var signatures = new List<int>();

        if (limit == 0) {
            limit = buffer.Length;
        } else {
            signatures.EnsureCapacity(limit);
        }

        var ptr = -signature.Length;
        while (ptr < stop) {
            ptr = FindSignature(buffer[(ptr + signature.Length)..], signature);
            if (ptr == -1) {
                break;
            }

            signatures.Add(ptr);

            if (limit-- == 0) {
                break;
            }
        }

        return signatures;
    }

    public static int FindSignatureReverse(Span<byte> buffer, Span<short> signature, int start = -1) {
        if (signature.Length == 0) {
            return -1;
        }

        if (start == -1 || start + signature.Length > buffer.Length) {
            start = buffer.Length - signature.Length;
        }

        for (var ptr = start; ptr > 0; --ptr) {
            var found = true;
            for (var i = 0; i < signature.Length; ++i) {
                var b = signature[i];
                unchecked {
                    if (b != -1 && (byte) b != buffer[ptr + i]) {
                        found = false;
                        break;
                    }
                }
            }

            if (found) {
                return ptr;
            }
        }

        return -1;
    }

    public static List<int> FindSignaturesReverse(Span<byte> buffer, Span<short> signature, int stop = -1, int start = -1, int limit = 0) {
        if (signature.Length == 0) {
            return new List<int>(0);
        }

        if (stop == -1) {
            stop = 0;
        }

        if (start == -1 || start + signature.Length > buffer.Length) {
            start = buffer.Length;
        }

        var signatures = new List<int>();
        if (limit == 0) {
            limit = buffer.Length;
        } else {
            signatures.EnsureCapacity(limit);
        }

        var ptr = start;
        while (ptr > stop) {
            ptr = FindSignatureReverse(buffer, signature, ptr - signature.Length);
            if (ptr == -1) {
                break;
            }

            signatures.Add(ptr);

            if (limit-- == 0) {
                break;
            }
        }

        return signatures;
    }

    public static Span<short> CreateSignature(string signatureTemplate) {
        var signatureOctets = signatureTemplate.ToHexOctets();
        if (signatureOctets.Length < 1) {
            return Span<short>.Empty;
        }

        Span<short> signature = new short[signatureOctets.Length];
        for (var i = 0; i < signatureOctets.Length; ++i) {
            if (signatureOctets[i] == "??") {
                signature[i] = -1;
            } else {
                signature[i] = byte.Parse(signatureOctets[i], NumberStyles.HexNumber);
            }
        }

        return signature;
    }

    public static int FindSignature(Span<byte> buffer, string signatureTemplate) {
        var signature = CreateSignature(signatureTemplate);
        return FindSignature(buffer, signature);
    }

    public static List<int> FindSignatures(Span<byte> buffer, string signatureTemplate, int stop = -1, int limit = 0) {
        var signature = CreateSignature(signatureTemplate);
        return FindSignatures(buffer, signature, stop, limit);
    }

    public static int FindSignatureReverse(Span<byte> buffer, string signatureTemplate, int start = -1) {
        var signature = CreateSignature(signatureTemplate);
        return FindSignatureReverse(buffer, signature, start);
    }

    public static List<int> FindSignaturesReverse(Span<byte> buffer, string signatureTemplate, int stop = -1, int start = -1, int limit = 0) {
        var signature = CreateSignature(signatureTemplate);
        return FindSignaturesReverse(buffer, signature, stop, start, limit);
    }
}
