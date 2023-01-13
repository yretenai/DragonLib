using System.Numerics;
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
        var directory = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrEmpty(directory)) {
            return;
        }

        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }
    }

    public static void EnsureDirectoriesExists(this IEnumerable<string> path) {
        var paths = path.Select(Path.GetFullPath).Select(Path.GetDirectoryName).Distinct().ToArray();
        foreach (var directory in paths) {
            if (string.IsNullOrEmpty(directory)) {
                continue;
            }

            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }
        }
    }

    public static Span<byte> AsBytes<T>(this Span<T> data) where T : unmanaged => MemoryMarshal.AsBytes(data);

    public static ReadOnlySpan<byte> AsBytes<T>(this ReadOnlySpan<T> data) where T : unmanaged => MemoryMarshal.AsBytes(data);

    public static string? ReadString<T>(this Span<T> data, Encoding encoding, T terminator, int limit = -1) where T : unmanaged, IEquatable<T> {
        if (limit > -1) {
            data = data[..limit];
        }

        if (data.Length == 0 || data[0].Equals(terminator)) {
            return null;
        }

        var index = data.IndexOf(terminator);
        if (index <= -1) {
            index = data.Length;
        }

        return encoding.GetString(data[..index].AsBytes());
    }

    public static string ReadStringNonNull<T>(this Span<T> data, Encoding encoding, T terminator, int limit = -1) where T : unmanaged, IEquatable<T> {
        var str = data.ReadString(encoding, terminator, limit);
        return str ?? string.Empty;
    }

    public static string? ReadASCIIString(this Span<byte> data, int limit = -1) => ReadString(data, Encoding.ASCII, (byte) 0, limit);
    public static string ReadASCIIStringNonNull(this Span<byte> data) => ReadASCIIString(data) ?? string.Empty;
    public static string? ReadASCIIString(this Span<sbyte> data, int limit = -1) => ReadASCIIString(MemoryMarshal.Cast<sbyte, byte>(data), limit);
    public static string ReadASCIIStringNonNull(this Span<sbyte> data, int limit = -1) => ReadASCIIString(MemoryMarshal.Cast<sbyte, byte>(data), limit) ?? string.Empty;
    public static string? ReadUTF8String(this Span<byte> data, int limit = -1) => ReadString(data, Encoding.UTF8, (byte) 0, limit);
    public static string ReadUTF8StringNonNull(this Span<byte> data) => ReadUTF8String(data) ?? string.Empty;
    public static string? ReadUTF8String(this Span<sbyte> data, int limit = -1) => ReadUTF8String(MemoryMarshal.Cast<sbyte, byte>(data), limit);
    public static string ReadUTF8StringNonNull(this Span<sbyte> data, int limit = -1) => ReadUTF8String(MemoryMarshal.Cast<sbyte, byte>(data), limit) ?? string.Empty;
    public static string? ReadUTF16String(this Span<char> data, int limit = -1, bool bigEndian = false) => ReadString(data, bigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode, '\0', limit);
    public static string ReadUTF16StringNonNull(this Span<char> data, int limit = -1, bool bigEndian = false) => ReadUTF16String(data, limit, bigEndian) ?? string.Empty;
    public static string? ReadUTF16String(this Span<ushort> data, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<ushort, char>(data), limit, bigEndian);
    public static string ReadUTF16StringNonNull(this Span<ushort> data, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<ushort, char>(data), limit, bigEndian) ?? string.Empty;
    public static string? ReadUTF16String(this Span<short> data, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<short, char>(data), limit, bigEndian);
    public static string ReadUTF16StringNonNull(this Span<short> data, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<short, char>(data), limit, bigEndian) ?? string.Empty;
    public static string? ReadUTF16String(this Span<byte> data, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<byte, char>(data), limit, bigEndian);
    public static string ReadUTF16StringNonNull(this Span<byte> data, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<byte, char>(data), limit, bigEndian) ?? string.Empty;
    public static string? ReadUTF32String(this Span<uint> data, int limit = -1) => ReadString(data, Encoding.UTF32, 0u, limit);
    public static string ReadUTF32StringNonNull(this Span<uint> data, int limit = -1) => ReadUTF32String(data, limit) ?? string.Empty;
    public static string? ReadUTF32String(this Span<int> data, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<int, uint>(data), limit);
    public static string ReadUTF32StringNonNull(this Span<int> data, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<int, uint>(data), limit) ?? string.Empty;
    public static string? ReadUTF32String(this Span<char> data, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<char, uint>(data), limit);
    public static string ReadUTF32StringNonNull(this Span<char> data, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<char, uint>(data), limit) ?? string.Empty;
    public static string? ReadUTF32String(this Span<byte> data, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<byte, uint>(data), limit);
    public static string ReadUTF32StringNonNull(this Span<byte> data, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<byte, uint>(data), limit) ?? string.Empty;

    public static int Align(this int value, int n) => unchecked(value + (n - 1)) & ~(n - 1);

    public static uint Align(this uint value, uint n) => unchecked(value + (n - 1)) & ~(n - 1);

    public static long Align(this long value, long n) => unchecked(value + (n - 1)) & ~(n - 1);

    public static ulong Align(this ulong value, ulong n) => unchecked(value + (n - 1)) & ~(n - 1);

    public static string UnixPath(this string path, bool isDir) {
        if (string.IsNullOrEmpty(path)) {
            return string.Empty;
        }

        var p = path.Replace('\\', '/');
        return isDir ? p + "/" : p;
    }

    public static string[] ToHexOctets(this string? input) {
        var cleaned = input?.Replace(" ", string.Empty, StringComparison.Ordinal).Trim();
        if (string.IsNullOrEmpty(cleaned)) {
            return Array.Empty<string>();
        }

        if (cleaned.Length % 2 != 0) {
            throw new FormatException("Input string must have an even number of characters.");
        }

        return Enumerable.Range(0, cleaned.Length)
            .Where(x => x % 2 == 0)
            .Select(x => cleaned.Substring(x, 2))
            .ToArray();
    }

    public static string ToHexString<T>(this Span<T> input, string separator = "", string prefix = "") where T : IConvertible {
        var formatter = $"x{Marshal.SizeOf<T>() * 2}";
        var sb = new StringBuilder();
        if (prefix.Length > 0 && separator.Length == 0) {
            sb.Append(prefix);
        }

        for (var index = 0; index < input.Length; index++) {
            var b = input[index];
            if (prefix.Length > 0 && separator.Length != 0) {
                sb.Append(prefix);
            }

            sb.Append(b.ToUInt64(null).ToString(formatter));

            if(separator.Length > 0 && index < input.Length - 1) {
                sb.Append(separator);
            }
        }

        return sb.ToString();
    }

    public static byte[] ToBytes(this string? input, int hextetLength = 2) {
        if (hextetLength is > 2 or < 1) {
            throw new FormatException("Hextet length must be 1 or 2.");
        }

        var cleaned = input?.Replace(" ", string.Empty, StringComparison.Ordinal).Replace(", ", string.Empty, StringComparison.Ordinal).Trim();
        if (string.IsNullOrWhiteSpace(cleaned)) {
            return Array.Empty<byte>();
        }

        if (cleaned.Length % hextetLength != 0) {
            throw new FormatException("Input string must have an even number of characters.");
        }

        if (cleaned.StartsWith("0x")) {
            cleaned = cleaned[2..];
        }

        return Enumerable.Range(0, cleaned.Length)
            .Where(x => x % hextetLength == 0)
            .Select(x => byte.Parse(cleaned.Substring(x, hextetLength), NumberStyles.HexNumber))
            .ToArray();
    }

    public static int DivideByRoundUp(this int value, int divisor) => (int) Math.Ceiling((double) value / divisor);

    public static T Clamp<T, V>(this V value) where T : unmanaged, INumberBase<T> where V : unmanaged, INumberBase<V> {
        return T.CreateSaturating(value);
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

    public static T[] SplitFlags<T>(this T flags) where T : Enum {
        var v = Convert.ToUInt64(flags);
        if (v == 0) {
            return Array.Empty<T>();
        }

        var t = new T[BitOperations.PopCount(v)];

        var i = 0;
        for (var j = 0; j < 64; ++j) {
            if ((v & (1UL << j)) != 0) {
                t[i++] = (T) Enum.ToObject(typeof(T), 1UL << j);
            }
        }

        return t;
    }
}
