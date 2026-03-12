// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace DragonLib.Extensions;

public static class StringExtensions {
	extension(string input) {
		public string SanitizeFilename(char replaceChar = '_') {
			var illegal = Path.GetInvalidFileNameChars();

			return illegal.Aggregate(input, (current, ch) => current.Replace(ch, replaceChar));
		}

		public string SanitizeDirname(char replaceChar = '_') {
			var illegal = Path.GetInvalidPathChars();

			return illegal.Aggregate(input, (current, ch) => current.Replace(ch, replaceChar));
		}

		public string SanitizeTraversal(char replaceChar = '_') {
			var illegal = Path.GetInvalidPathChars();

			var value = illegal.Aggregate(input, (current, ch) => current.Replace(ch, replaceChar)).Replace("/../", "/", StringComparison.Ordinal);
			if (OperatingSystem.IsWindows()) {
				value = value.Replace("/", @"\", StringComparison.Ordinal).Replace(@"\..\", @"\", StringComparison.Ordinal);
			}

			return value.TrimStart('/', '\\', '.');
		}

		public void EnsureDirectoryExists() {
			var fullPath = Path.GetFullPath(input);
			var directory = Path.GetDirectoryName(fullPath);
			if (string.IsNullOrEmpty(directory)) {
				return;
			}

			Directory.CreateDirectory(directory);
		}


		public string UnixPath(bool isDir) {
			if (string.IsNullOrEmpty(input)) {
				return string.Empty;
			}

			var p = input.Replace('\\', '/');
			return isDir ? p + "/" : p;
		}

		public string[] ToHexOctets() {
			var cleaned = input.Replace(" ", string.Empty, StringComparison.Ordinal).Trim();
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

		public string Quoted(char quoteValue = '"', char escapeValue = '\\', bool escapeUnicode = false, bool wrapQuotes = true) {
			if (string.IsNullOrWhiteSpace(input)) {
				return !wrapQuotes ? string.Empty : $"{quoteValue}{quoteValue}";
			}

			var sb = new StringBuilder();
			if (wrapQuotes) {
				sb.Append(quoteValue);
			}

			var utf32 = Encoding.UTF32;
			var text = utf32.GetBytes(input).AsSpan().As<byte, int>();
			var keyValues = utf32.GetBytes($"{quoteValue}{escapeValue}").AsSpan().As<byte, int>();
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

		public string ReplaceIfPresent(char from, char to) {
			if (input.Contains(from, StringComparison.Ordinal)) {
				return input.Replace(from, to);
			}

			return input;
		}

		public string ReplaceIfPresent(string from, string to, StringComparison? comparison = null) {
			if (input.Contains(from, comparison ?? StringComparison.Ordinal)) {
				return input.Replace(from, to, comparison ?? StringComparison.Ordinal);
			}

			return input;
		}
	}
}
