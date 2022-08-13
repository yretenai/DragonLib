using System.Runtime.InteropServices;

namespace DragonLib;

public static class Extensions {
    private static readonly sbyte[] SignedNibbles = { 0, 1, 2, 3, 4, 5, 6, 7, -8, -7, -6, -5, -4, -3, -2, -1 };

    private static readonly string[] BytePoints = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB" };

    public static Span<T> Clone<T>(this Span<T> span) {
        var clone = new T[span.Length];
        span.CopyTo(clone);
        return new Span<T>(clone);
    }

    public static string SanitizeFilename(this string path, char replaceChar = '_') {
        var illegal = Path.GetInvalidFileNameChars();

        return illegal.Aggregate(path, (current, ch) => current.Replace(ch, replaceChar));
    }

    public static string SanitizeDirname(this string path, char replaceChar = '_') {
        var illegal = Path.GetInvalidPathChars();

        return illegal.Aggregate(path, (current, ch) => current.Replace(ch, replaceChar));
    }

    public static void EnsureDirectoryExists(this string path) {
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath)!;
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }
    }

    public static Span<byte> AsBytes<T>(this Span<T> data) where T : unmanaged => MemoryMarshal.AsBytes(data);

    public static ReadOnlySpan<byte> AsBytes<T>(this ReadOnlySpan<T> data) where T : unmanaged => MemoryMarshal.AsBytes(data);

    public static string? ReadString(this Span<byte> data, Encoding? encoding = null) {
        if (data.Length == 0 || data[0] == 0) {
            return null;
        }

        var index = data.IndexOf<byte>(0);
        if (index <= -1) {
            index = data.Length;
        }

        return (encoding ?? Encoding.UTF8).GetString(data[..index]);
    }

    public static string ReadStringNonNull(this Span<byte> data, Encoding? encoding = null) => ReadString(data, encoding) ?? string.Empty;

    public static string? ReadString(this Span<char> data, Encoding? encoding = null) => MemoryMarshal.AsBytes(data).ReadString(encoding);

    public static string ReadStringNonNull(this Span<char> data, Encoding? encoding = null) => MemoryMarshal.AsBytes(data).ReadStringNonNull(encoding);

    public static int Align(this int value, int n) => unchecked(value + (n - 1)) & ~(n - 1);

    public static uint Align(this uint value, uint n) => unchecked(value + (n - 1)) & ~(n - 1);

    public static long Align(this long value, long n) => unchecked(value + (n - 1)) & ~(n - 1);

    public static ulong Align(this ulong value, ulong n) => unchecked(value + (n - 1)) & ~(n - 1);

    public static string UnixPath(this string path, bool isDir) {
        var p = path.Replace('\\', '/');
        return isDir ? p + "/" : p;
    }

    public static string[] ToHexOctets(this string? input) {
        var cleaned = input?.Replace(" ", string.Empty).Trim();
        if (cleaned == null || cleaned.Length % 2 != 0) {
            return Array.Empty<string>();
        }

        return Enumerable.Range(0, cleaned.Length)
            .Where(x => x % 2 == 0)
            .Select(x => cleaned.Substring(x, 2))
            .ToArray();
    }

    public static string ToHexString(this byte[] input, string separator = "", string prefix = "") {
        return string.Join(separator, input.Select(x => prefix + x.ToString("x2")));
    }

    public static string ToHexString(this Span<byte> input, string separator = "", string prefix = "") {
        return string.Join(separator, input.ToArray().Select(x => prefix + x.ToString("x2")));
    }

    public static byte[] ToBytes(this string? input, int hextetLength = 2) {
        var cleaned = input?.Replace(" ", string.Empty).Replace(", ", string.Empty).Trim();
        if (cleaned == null || cleaned.Length % hextetLength != 0) {
            return Array.Empty<byte>();
        }

        return Enumerable.Range(0, cleaned.Length)
            .Where(x => x % hextetLength == 0)
            .Select(x => byte.Parse(cleaned.Substring(x, hextetLength), NumberStyles.HexNumber))
            .ToArray();
    }

    public static int FindPointerFromSignature(this Span<byte> buffer, string signatureTemplate) {
        var signatureOctets = signatureTemplate.ToHexOctets();
        if (signatureOctets.Length < 1) {
            return -1;
        }

        var signature = signatureOctets.Select(x => x == "??" ? (byte?) null : Convert.ToByte(x, 16)).ToArray();
        for (var ptr = 0; ptr < buffer.Length - signature.Length; ++ptr) {
            var slice = buffer.Slice(ptr, signature.Length);
            var found = true;
            for (var i = 0; i < signature.Length; ++i) {
                var b = signature[i];
                if (b != null && b != slice[i]) {
                    found = false;
                }
            }

            if (found) {
                return ptr;
            }
        }

        return -1;
    }

    public static int
        FindPointerFromSignatureReverse(this Span<byte> buffer, string signatureTemplate, int start = -1) {
        var signatureOctets = signatureTemplate.ToHexOctets();
        if (signatureOctets.Length < 1) {
            return -1;
        }

        var signature = signatureOctets.Select(x => x == "??" ? (byte?) null : Convert.ToByte(x, 16)).ToArray();
        if (start == -1 || start + signature.Length > buffer.Length) {
            start = buffer.Length - signature.Length;
        }

        for (var ptr = start; ptr > 0; --ptr) {
            var slice = buffer.Slice(ptr, signature.Length);
            var found = true;
            for (var i = 0; i < signature.Length; ++i) {
                var b = signature[i];
                if (b != null && b != slice[i]) {
                    found = false;
                }
            }

            if (found) {
                return ptr;
            }
        }

        return -1;
    }

    public static int FindFirstAlphanumericSequence(this Span<byte> buffer, int length = 1) {
        for (var ptr = 0; ptr < buffer.Length - length; ++ptr) {
            var slice = buffer.Slice(ptr, length);
            if (slice.ToArray().All(x => x is >= 0x30 and <= 0x7F)) {
                return ptr;
            }
        }

        return -1;
    }

    public static int DivideByRoundUp(this int value, int divisor) => (int) Math.Ceiling((double) value / divisor);

    public static short ShortClamp(this int value) {
        return value switch {
            > short.MaxValue => short.MaxValue,
            < short.MinValue => short.MinValue,
            _ => (short) value,
        };
    }

    public static byte GetHighNibble(this byte value) => (byte) ((value >> 4) & 0xF);

    public static byte GetLowNibble(this byte value) => (byte) (value & 0xF);

    public static sbyte GetHighNibbleSigned(this byte value) => SignedNibbles[value.GetHighNibble()];

    public static sbyte GetLowNibbleSigned(this byte value) => SignedNibbles[value.GetLowNibble()];

    public static string GetHumanReadableBytes(this ulong bytes) {
        for (var i = 0; i < BytePoints.Length; ++i) {
            var divisor = Math.Pow(0x400, i);
            var nextDivisor = Math.Pow(0x400, i + 1);
            if (!(bytes < nextDivisor) && i != BytePoints.Length - 1) {
                continue;
            }

            var normalized = Math.Floor(bytes / (divisor / 10)) / 10;
            return $"{normalized} {BytePoints[i]}";
        }

        return $"{bytes} B";
    }

    public static string GetHumanReadableBytes(this long bytes) => GetHumanReadableBytes((ulong) bytes);

    public static string GetHumanReadableBytes(this int bytes) => GetHumanReadableBytes((ulong) bytes);

    public static string GetHumanReadableBytes(this uint bytes) => GetHumanReadableBytes((ulong) bytes);
}
