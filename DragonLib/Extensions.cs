using System.Numerics;
using System.Runtime.InteropServices;

namespace DragonLib;

public static class Extensions {
    private static readonly sbyte[] SignedNibbles = { 0, 1, 2, 3, 4, 5, 6, 7, -8, -7, -6, -5, -4, -3, -2, -1 };
    private static readonly string[] BytePoints = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB" };
    public const long OneKiB = 1024;
    public const long OneMiB = OneKiB * 1024;
    public const long OneGiB = OneMiB * 1024;
    public const long OneTiB = OneGiB * 1024;
    public const long OnePiB = OneTiB * 1024;
    public const long OneEiB = OnePiB * 1024;
    public static TimeSpan OneMicrosecond { get; } = TimeSpan.FromMicroseconds(1);
    public static TimeSpan OneMilisecond { get; } = TimeSpan.FromMilliseconds(1);
    public static TimeSpan OneSecond { get; } = TimeSpan.FromSeconds(1);
    public static TimeSpan OneMinute { get; } = TimeSpan.FromMinutes(1);
    public static TimeSpan OneHour { get; } = TimeSpan.FromHours(1);
    public static TimeSpan OneDay { get; } = TimeSpan.FromDays(1);
    public static TimeSpan OneMonth { get; } = TimeSpan.FromDays(30);
    public static TimeSpan OneYear { get; } = TimeSpan.FromDays(356);
    public static TimeSpan OneDecade { get; } = OneYear * 10;
    public static TimeSpan OneCentury { get; } = OneDecade * 10;
    public static TimeSpan OneKiloyear { get; } = OneCentury * 10;

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

    public static void EnsureDirectoryRootExists(this string path) {
        var directory = Path.GetFullPath(path);
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

    public static string? ReadString<T>(this Span<T> data, Encoding encoding, T terminator, out int length, int limit = -1) where T : unmanaged, IEquatable<T> {
        if (limit > -1) {
            data = data[..limit];
        }

        if (data.Length == 0 || data[0].Equals(terminator)) {
            length = 0;
            return null;
        }

        length = data.IndexOf(terminator);
        if (length <= -1) {
            length = data.Length;
        }

        return encoding.GetString(data[..length].AsBytes());
    }

    public static string ReadStringNonNull<T>(this Span<T> data, Encoding encoding, T terminator, out int index, int limit = -1) where T : unmanaged, IEquatable<T> {
        var str = data.ReadString(encoding, terminator, out index, limit);
        return str ?? string.Empty;
    }

    public static string? ReadASCIIString(this Span<byte> data, int limit = -1) => ReadString(data, Encoding.ASCII, (byte) 0, out _, limit);
    public static string ReadASCIIStringNonNull(this Span<byte> data, int limit = -1) => ReadASCIIString(data, limit) ?? string.Empty;
    public static string? ReadASCIIString(this Span<sbyte> data, int limit = -1) => ReadASCIIString(MemoryMarshal.Cast<sbyte, byte>(data), limit);
    public static string ReadASCIIStringNonNull(this Span<sbyte> data, int limit = -1) => ReadASCIIString(MemoryMarshal.Cast<sbyte, byte>(data), limit) ?? string.Empty;
    public static string? ReadUTF8String(this Span<byte> data, int limit = -1) => ReadString(data, Encoding.UTF8, (byte) 0, out _, limit);
    public static string ReadUTF8StringNonNull(this Span<byte> data, int limit = -1) => ReadUTF8String(data, limit) ?? string.Empty;
    public static string? ReadUTF8String(this Span<sbyte> data, int limit = -1) => ReadUTF8String(MemoryMarshal.Cast<sbyte, byte>(data), limit);
    public static string ReadUTF8StringNonNull(this Span<sbyte> data, int limit = -1) => ReadUTF8String(MemoryMarshal.Cast<sbyte, byte>(data), limit) ?? string.Empty;
    public static string? ReadUTF16String(this Span<char> data, int limit = -1, bool bigEndian = false) => ReadString(data, bigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode, '\0', out _, limit);
    public static string ReadUTF16StringNonNull(this Span<char> data, int limit = -1, bool bigEndian = false) => ReadUTF16String(data, limit, bigEndian) ?? string.Empty;
    public static string? ReadUTF16String(this Span<ushort> data, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<ushort, char>(data), limit, bigEndian);
    public static string ReadUTF16StringNonNull(this Span<ushort> data, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<ushort, char>(data), limit, bigEndian) ?? string.Empty;
    public static string? ReadUTF16String(this Span<short> data, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<short, char>(data), limit, bigEndian);
    public static string ReadUTF16StringNonNull(this Span<short> data, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<short, char>(data), limit, bigEndian) ?? string.Empty;
    public static string? ReadUTF16String(this Span<byte> data, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<byte, char>(data), limit, bigEndian);
    public static string ReadUTF16StringNonNull(this Span<byte> data, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<byte, char>(data), limit, bigEndian) ?? string.Empty;
    public static string? ReadUTF32String(this Span<uint> data, int limit = -1) => ReadString(data, Encoding.UTF32, 0u, out _, limit);
    public static string ReadUTF32StringNonNull(this Span<uint> data, int limit = -1) => ReadUTF32String(data, limit) ?? string.Empty;
    public static string? ReadUTF32String(this Span<int> data, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<int, uint>(data), limit);
    public static string ReadUTF32StringNonNull(this Span<int> data, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<int, uint>(data), limit) ?? string.Empty;
    public static string? ReadUTF32String(this Span<char> data, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<char, uint>(data), limit);
    public static string ReadUTF32StringNonNull(this Span<char> data, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<char, uint>(data), limit) ?? string.Empty;
    public static string? ReadUTF32String(this Span<byte> data, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<byte, uint>(data), limit);
    public static string ReadUTF32StringNonNull(this Span<byte> data, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<byte, uint>(data), limit) ?? string.Empty;
    public static string? ReadASCIIString(this Span<byte> data, out int size, int limit = -1) => ReadString(data, Encoding.ASCII, (byte) 0, out size, limit);
    public static string ReadASCIIStringNonNull(this Span<byte> data, out int size, int limit = -1) => ReadASCIIString(data, out size, limit) ?? string.Empty;
    public static string? ReadASCIIString(this Span<sbyte> data, out int size, int limit = -1) => ReadASCIIString(MemoryMarshal.Cast<sbyte, byte>(data), out size, limit);
    public static string ReadASCIIStringNonNull(this Span<sbyte> data, out int size, int limit = -1) => ReadASCIIString(MemoryMarshal.Cast<sbyte, byte>(data), out size, limit) ?? string.Empty;
    public static string? ReadUTF8String(this Span<byte> data, out int size, int limit = -1) => ReadString(data, Encoding.UTF8, (byte) 0, out size, limit);
    public static string ReadUTF8StringNonNull(this Span<byte> data, out int size, int limit = -1) => ReadUTF8String(data, out size, limit) ?? string.Empty;
    public static string? ReadUTF8String(this Span<sbyte> data, out int size, int limit = -1) => ReadUTF8String(MemoryMarshal.Cast<sbyte, byte>(data), out size, limit);
    public static string ReadUTF8StringNonNull(this Span<sbyte> data, out int size, int limit = -1) => ReadUTF8String(MemoryMarshal.Cast<sbyte, byte>(data), out size, limit) ?? string.Empty;
    public static string? ReadUTF16String(this Span<char> data, out int size, int limit = -1, bool bigEndian = false) => ReadString(data, bigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode, '\0', out size, limit);
    public static string ReadUTF16StringNonNull(this Span<char> data, out int size, int limit = -1, bool bigEndian = false) => ReadUTF16String(data, out size, limit, bigEndian) ?? string.Empty;
    public static string? ReadUTF16String(this Span<ushort> data, out int size, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<ushort, char>(data), out size, limit, bigEndian);
    public static string ReadUTF16StringNonNull(this Span<ushort> data, out int size, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<ushort, char>(data), out size, limit, bigEndian) ?? string.Empty;
    public static string? ReadUTF16String(this Span<short> data, out int size, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<short, char>(data), out size, limit, bigEndian);
    public static string ReadUTF16StringNonNull(this Span<short> data, out int size, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<short, char>(data), out size, limit, bigEndian) ?? string.Empty;
    public static string? ReadUTF16String(this Span<byte> data, out int size, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<byte, char>(data), out size, limit, bigEndian);
    public static string ReadUTF16StringNonNull(this Span<byte> data, out int size, int limit = -1, bool bigEndian = false) => ReadUTF16String(MemoryMarshal.Cast<byte, char>(data), out size, limit, bigEndian) ?? string.Empty;
    public static string? ReadUTF32String(this Span<uint> data, out int size, int limit = -1) => ReadString(data, Encoding.UTF32, 0u, out size, limit);
    public static string ReadUTF32StringNonNull(this Span<uint> data, out int size, int limit = -1) => ReadUTF32String(data, out size, limit) ?? string.Empty;
    public static string? ReadUTF32String(this Span<int> data, out int size, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<int, uint>(data), out size, limit);
    public static string ReadUTF32StringNonNull(this Span<int> data, out int size, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<int, uint>(data), out size, limit) ?? string.Empty;
    public static string? ReadUTF32String(this Span<char> data, out int size, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<char, uint>(data), out size, limit);
    public static string ReadUTF32StringNonNull(this Span<char> data, out int size, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<char, uint>(data), out size, limit) ?? string.Empty;
    public static string? ReadUTF32String(this Span<byte> data, out int size, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<byte, uint>(data), out size, limit);
    public static string ReadUTF32StringNonNull(this Span<byte> data, out int size, int limit = -1) => ReadUTF32String(MemoryMarshal.Cast<byte, uint>(data), out size, limit) ?? string.Empty;

    public static int Align(this int value, int n) => unchecked(value + (n - 1)) & ~(n - 1);

    public static uint Align(this uint value, uint n) => unchecked(value + (n - 1)) & ~(n - 1);

    public static long Align(this long value, long n) => unchecked(value + (n - 1)) & ~(n - 1);

    public static ulong Align(this ulong value, ulong n) => unchecked(value + (n - 1)) & ~(n - 1);

    public static void Align(this Stream stream, long n) {
        if (stream.CanWrite && stream.Position.Align(n) >= stream.Length) {
            stream.SetLength(stream.Length.Align(n));
            stream.Position = stream.Length;
        } else {
            stream.Position = stream.Position.Align(n);
        }
    }

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

    public static string GetHumanReadableTime(this TimeSpan time, bool shortForm = false) {
        long amount;
        string metric;
        if (time >= OneKiloyear) {
            amount = (long) Math.Floor(time / OneKiloyear);
            metric = shortForm ? "kyr" : "kiloyear";
        } else if (time >= OneCentury) {
            amount = (long) Math.Floor(time / OneCentury);
            metric = shortForm ? "c" : "century";
        } else if (time >= OneDecade) {
            amount = (long) Math.Floor(time / OneDecade);
            metric = shortForm ? "s" : "decade";
        } else if (time >= OneYear) {
            amount = (long) Math.Floor(time / OneYear);
            metric = shortForm ? "y" : "year";
        } else if (time >= OneMonth) {
            amount = (long) Math.Floor(time / OneMonth);
            metric = shortForm ? "mo" : "month";
        } else if (time >= OneDay) {
            amount = (long) Math.Floor(time / OneDay);
            metric = shortForm ? "d" : "day";
        } else if (time >= OneHour) {
            amount = (long) Math.Floor(time / OneHour);
            metric = shortForm ? "h" : "hour";
        } else if (time >= OneMinute) {
            amount = (long) Math.Floor(time / OneMinute);
            metric = shortForm ? "m" : "minute";
        } else if (time >= OneSecond) {
            amount = (long) Math.Floor(time / OneSecond);
            metric = shortForm ? "s" : "second";
        } else if (time >= OneMilisecond) {
            amount = (long) Math.Floor(time / OneMilisecond);
            metric = shortForm ? "ms" : "milisecond";
        } else if (time >= OneMicrosecond) {
            amount = (long) Math.Floor(time / OneMicrosecond);
            metric = shortForm ? "us" : "microsecond";
        } else {
            amount = time.Ticks;
            metric = shortForm ? "t" : "tick";
        }

        if (shortForm) {
            return $"{amount}{metric}";
        }

        if (amount != 1) {
            metric += "s";
        }

        return $"{amount} {metric}";
    }

    public static long KiB(this int value) {
        return OneKiB * value;
    }

    public static long MiB(this int value) {
        return OneMiB * value;
    }

    public static long GiB(this int value) {
        return OneGiB * value;
    }

    public static long TiB(this int value) {
        return OneTiB * value;
    }

    public static long PiB(this int value) {
        return OnePiB * value;
    }

    public static long EiB(this int value) {
        return OneEiB * value;
    }

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

    public static string GetHumanReadableBytes(this long bytes) {
        var value = bytes;
        if (bytes < 0) {
            value = 0 - bytes;
        }

        var amount = GetHumanReadableBytes((ulong) value);
        return bytes < 0 ? "-" + amount : amount;
    }

    public static string GetHumanReadableBytes(this int bytes) => GetHumanReadableBytes((long) bytes);

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
