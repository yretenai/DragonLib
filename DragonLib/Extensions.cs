// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using System.Runtime.InteropServices;

namespace DragonLib;

public static class Extensions {
	public const long OneKiB = 1024;
	public const long OneMiB = OneKiB * 1024;
	public const long OneGiB = OneMiB * 1024;
	public const long OneTiB = OneGiB * 1024;
	public const long OnePiB = OneTiB * 1024;
	public const long OneEiB = OnePiB * 1024;
	private static readonly sbyte[] SignedNibbles = [0, 1, 2, 3, 4, 5, 6, 7, -8, -7, -6, -5, -4, -3, -2, -1];
	private static readonly string[] BytePoints = ["B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB"];
	public static TimeSpan OneNanosecond { get; } = TimeSpan.FromMicroseconds(0.001);
	public static TimeSpan OneMicrosecond { get; } = TimeSpan.FromMicroseconds(1);
	public static TimeSpan OneMillisecond { get; } = TimeSpan.FromMilliseconds(1);
	public static TimeSpan OneSecond { get; } = TimeSpan.FromSeconds(1);
	public static TimeSpan OneMinute { get; } = TimeSpan.FromMinutes(1);
	public static TimeSpan OneHour { get; } = TimeSpan.FromHours(1);
	public static TimeSpan OneDay { get; } = TimeSpan.FromDays(1);
	public static TimeSpan OneMonth { get; } = TimeSpan.FromDays(30);
	public static TimeSpan OneYear { get; } = TimeSpan.FromDays(356);
	public static TimeSpan OneDecade { get; } = OneYear * 10;
	public static TimeSpan OneCentury { get; } = OneDecade * 10;
	public static TimeSpan OneKiloyear { get; } = OneCentury * 10;

	public static Span<T> Clone<T>(this ReadOnlySpan<T> span) {
		var clone = new T[span.Length];
		span.CopyTo(clone);
		return new Span<T>(clone);
	}

	public static Span<T> Clone<T>(this Span<T> span) => Clone((ReadOnlySpan<T>) span);

	public static string SanitizeFilename(this string path, char replaceChar = '_') {
		var illegal = Path.GetInvalidFileNameChars();

		return illegal.Aggregate(path, (current, ch) => current.Replace(ch, replaceChar));
	}

	public static string SanitizeDirname(this string path, char replaceChar = '_') {
		var illegal = Path.GetInvalidPathChars();

		return illegal.Aggregate(path, (current, ch) => current.Replace(ch, replaceChar));
	}

	public static string SanitizeTraversal(this string path, char replaceChar = '_') {
		var illegal = Path.GetInvalidPathChars();

		var value = illegal.Aggregate(path, (current, ch) => current.Replace(ch, replaceChar)).Replace("/../", "/", StringComparison.Ordinal);
		if (OperatingSystem.IsWindows()) {
			value = value.Replace("/", @"\", StringComparison.Ordinal).Replace(@"\..\", @"\", StringComparison.Ordinal);
		}

		return value.TrimStart('/', '\\', '.');
	}

	public static void EnsureDirectoryExists(this string path) {
		var fullPath = Path.GetFullPath(path);
		var directory = Path.GetDirectoryName(fullPath);
		if (string.IsNullOrEmpty(directory)) {
			return;
		}

		Directory.CreateDirectory(directory);
	}

	public static void EnsureDirectoriesExists(this IEnumerable<string> path) {
		var paths = path.Select(Path.GetFullPath).Select(Path.GetDirectoryName).Distinct().ToArray();
		foreach (var directory in paths) {
			if (string.IsNullOrEmpty(directory)) {
				continue;
			}

			Directory.CreateDirectory(directory);
		}
	}

	public static Span<byte> AsBytes<T>(this Span<T> data) where T : struct => MemoryMarshal.AsBytes(data);

	public static ReadOnlySpan<byte> AsBytes<T>(this ReadOnlySpan<T> data) where T : struct => MemoryMarshal.AsBytes(data);

	public static Span<TTo> As<TFrom, TTo>(this Span<TFrom> data) where TFrom : struct where TTo : struct => MemoryMarshal.Cast<TFrom, TTo>(data);

	public static ReadOnlySpan<TTo> As<TFrom, TTo>(this ReadOnlySpan<TFrom> data) where TFrom : struct where TTo : struct => MemoryMarshal.Cast<TFrom, TTo>(data);

	public static Span<TTo> As<TTo>(this Span<byte> data) where TTo : struct => MemoryMarshal.Cast<byte, TTo>(data);

	public static ReadOnlySpan<TTo> As<TTo>(this ReadOnlySpan<byte> data) where TTo : struct => MemoryMarshal.Cast<byte, TTo>(data);

	public static string? ReadString<T>(this ReadOnlySpan<T> data, Encoding encoding, out int length, T terminator = default, int limit = -1) where T : struct, IEquatable<T> {
		if (limit > -1) {
			data = data[..limit];
		}

		if (data.Length == 0 || data[0].Equals(terminator)) {
			length = 0;
			return default;
		}

		length = data.IndexOf(terminator);
		if (length <= -1) {
			length = data.Length;
		}

		return encoding.GetString(data[..length].AsBytes());
	}

	public static string? ReadString<T>(this Span<T> data, Encoding encoding, out int length, T terminator = default, int limit = -1) where T : struct, IEquatable<T> => ReadString((ReadOnlySpan<T>) data, encoding, out length, terminator, limit);

	public static string? ReadString<T>(this ReadOnlySpan<T> data, Encoding encoding, int limit = -1) where T : struct, IEquatable<T> => ReadString(data, encoding, out _, default, limit);

	public static string? ReadString<T>(this Span<T> data, Encoding encoding, int limit = -1) where T : struct, IEquatable<T> => ReadString((ReadOnlySpan<T>) data, encoding, out _, default, limit);

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
			return [];
		}

		if (cleaned.Length % 2 != 0) {
			throw new FormatException("Input string must have an even number of characters.");
		}

		return Enumerable.Range(0, cleaned.Length)
						 .Where(x => x % 2 == 0)
						 .Select(x => cleaned.Substring(x, 2))
						 .ToArray();
	}

	[Obsolete("Use Convert.ToHexString")]
	public static string ToHexString<T>(this ReadOnlySpan<T> input, string separator = "", string prefix = "") where T : IConvertible {
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

			sb.Append(b.ToUInt64(default).ToString(formatter));

			if (separator.Length > 0 && index < input.Length - 1) {
				sb.Append(separator);
			}
		}

		return sb.ToString();
	}

	[Obsolete("Use Convert.FromHexString")]
	public static byte[] ToBytes(this string? input, int hextetLength = 2) {
		if (hextetLength is > 2 or < 1) {
			throw new FormatException("Hextet length must be 1 or 2.");
		}

		var cleaned = input?.Replace(" ", string.Empty, StringComparison.Ordinal).Replace(", ", string.Empty, StringComparison.Ordinal).Trim();
		if (string.IsNullOrWhiteSpace(cleaned)) {
			return [];
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

	public static T DivideByRoundUp<T>(this T left, T right) where T : IBinaryNumber<T>, IAdditionOperators<T, T, T> =>
		(left - T.One) / right + T.One;

	public static T Clamp<T, TValue>(this TValue value) where T : struct, INumberBase<T> where TValue : struct, INumberBase<TValue> => T.CreateSaturating(value);

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
		} else if (time >= OneMillisecond) {
			amount = (long) Math.Floor(time / OneMillisecond);
			metric = shortForm ? "ms" : "millisecond";
		} else if (time >= OneMicrosecond) {
			amount = (long) Math.Floor(time / OneMicrosecond);
			metric = shortForm ? "us" : "microsecond";
		} else if (time >= OneNanosecond) {
			amount = (long) Math.Floor(time / OneNanosecond);
			metric = shortForm ? "ns" : "nanosecond";
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

	public static long KiB(this int value) => OneKiB * value;

	public static long MiB(this int value) => OneMiB * value;

	public static long GiB(this int value) => OneGiB * value;

	public static long TiB(this int value) => OneTiB * value;

	public static long PiB(this int value) => OnePiB * value;

	public static long EiB(this int value) => OneEiB * value;

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
			return [];
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

	public static string Quoted(this string? value, char quoteValue = '"', char escapeValue = '\\', bool escapeUnicode = false, bool wrapQuotes = true) {
		if (string.IsNullOrWhiteSpace(value)) {
			return !wrapQuotes ? string.Empty : $"{quoteValue}{quoteValue}";
		}

		var sb = new StringBuilder();
		if (wrapQuotes) {
			sb.Append(quoteValue);
		}

		var utf32 = Encoding.UTF32;
		var text = utf32.GetBytes(value).AsSpan().As<int>();
		var keyValues = utf32.GetBytes($"{quoteValue}{escapeValue}").AsSpan().As<int>();
		var quote = keyValues[0];
		var escape = keyValues[1];
		// ReSharper disable twice ArrangeRedundantParentheses
		var charBuffer = (stackalloc char[5]);
		var chBuffer32 = 0;
		ReadOnlySpan<byte> chBuffer = MemoryMarshal.AsBytes(new Span<int>(ref chBuffer32));
		foreach (var ch in text) {
			if (ch == quote) {
				sb.Append($"{escapeValue}{quoteValue}");
				continue;
			}

			if (ch == escape) {
				sb.Append($"{escapeValue}{escapeValue}");
				continue;
			}

			switch (ch) {
				case '\0': sb.Append($"{escapeValue}0"); break;
				case '\a': sb.Append($"{escapeValue}a"); break;
				case '\b': sb.Append($"{escapeValue}b"); break;
				case '\f': sb.Append($"{escapeValue}f"); break;
				case '\n': sb.Append($"{escapeValue}n"); break;
				case '\r': sb.Append($"{escapeValue}r"); break;
				case '\t': sb.Append($"{escapeValue}t"); break;
				case '\v': sb.Append($"{escapeValue}v"); break;
				default:
					if ((escapeUnicode && ch is < 0x20 or > 0x7e) || CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.Control) {
						sb.Append($"{escapeValue}u{ch:x4}");
						continue;
					}

					chBuffer32 = ch;
					var n = utf32.GetChars(chBuffer, charBuffer);
					for (var i = 0; i < n; ++i) {
						sb.Append(charBuffer[i]);
					}

					break;
			}
		}

		if (wrapQuotes) {
			sb.Append(quoteValue);
		}

		return sb.ToString();
	}

	public static T? SafeDequeue<T>(this Queue<T> queue, T? fallback = default) => queue.Count == 0 ? fallback : queue.Dequeue();

	public static T GreatestCommonDivisor<T>(this T left, T right) where T : IBinaryInteger<T> {
		while (right != T.Zero) {
			var temp = right;
			right = left % right;
			left = temp;
		}

		return left;
	}

	public static T FirstMultipleOf<T>(this T value, T multiple) where T : IBinaryInteger<T> {
		var gcd = value.GreatestCommonDivisor(multiple);
		var k = multiple / gcd;
		return value * k;
	}

	public static T AsMagicConstant<T>(this string value) where T : unmanaged {
		var test = new T();
		Encoding.UTF8.GetBytes(value, MemoryMarshal.AsBytes(new Span<T>(ref test)));
		return test;
	}
}
