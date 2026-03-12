using System.Numerics;

namespace DragonLib.Extensions;

public static class NumberExtensions {
	public const long OneKiB = 1024;
	public const long OneMiB = OneKiB * 1024;
	public const long OneGiB = OneMiB * 1024;
	public const long OneTiB = OneGiB * 1024;
	public const long OnePiB = OneTiB * 1024;
	public const long OneEiB = OnePiB * 1024;
	private static readonly sbyte[] SignedNibbles = [0, 1, 2, 3, 4, 5, 6, 7, -8, -7, -6, -5, -4, -3, -2, -1];
	private static readonly string[] BytePoints = ["B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB"];

	extension(byte value) {
		public byte HighNibble => (byte) ((value >> 4) & 0xF);
		public byte LowNibble => (byte) (value & 0xF);
		public sbyte SignedHighNibble => SignedNibbles[value.HighNibble];
		public sbyte SignedLowNibble => SignedNibbles[value.LowNibble];
	}

	extension(int value) {
		public int Align(int n) => unchecked(value + (n - 1)) & ~(n - 1);
		public long KiB => OneKiB * value;
		public long MiB => OneMiB * value;
		public long GiB => OneGiB * value;
		public long TiB => OneTiB * value;
		public long PiB => OnePiB * value;
		public long EiB => OneEiB * value;

		public string HumanReadableBytes => ((ulong) value).HumanReadableBytes;
	}

	extension(uint value) {
		public uint Align(uint n) => unchecked(value + (n - 1)) & ~(n - 1);
		public long KiB => OneKiB * value;
		public long MiB => OneMiB * value;
		public long GiB => OneGiB * value;
		public long TiB => OneTiB * value;
		public long PiB => OnePiB * value;
		public long EiB => OneEiB * value;

		public string HumanReadableBytes => ((ulong) value).HumanReadableBytes;
	}

	extension(long value) {
		public long Align(long n) => unchecked(value + (n - 1)) & ~(n - 1);
		public long KiB => OneKiB * value;
		public long MiB => OneMiB * value;
		public long GiB => OneGiB * value;
		public long TiB => OneTiB * value;
		public long PiB => OnePiB * value;
		public long EiB => OneEiB * value;

		public string HumanReadableBytes {
			get {
				var v = value;
				if (value < 0) {
					v = 0 - value;
				}

				var amount = ((ulong) v).HumanReadableBytes;
				return value < 0 ? "-" + amount : amount;
			}
		}
	}

	extension(ulong value) {
		public ulong Align(ulong n) => unchecked(value + (n - 1)) & ~(n - 1);
		public ulong KiB => OneKiB * value;
		public ulong MiB => OneMiB * value;
		public ulong GiB => OneGiB * value;
		public ulong TiB => OneTiB * value;
		public ulong PiB => OnePiB * value;
		public ulong EiB => OneEiB * value;

		public string HumanReadableBytes {
			get {
				for (var i = 0; i < BytePoints.Length; ++i) {
					var divisor = Math.Pow(0x400, i);
					var nextDivisor = Math.Pow(0x400, i + 1);
					if (!(value < nextDivisor) && i != BytePoints.Length - 1) {
						continue;
					}

					var normalized = Math.Floor(value / (divisor / 10)) / 10;
					return $"{normalized} {BytePoints[i]}";
				}

				return $"{value} B";
			}
		}
	}

	extension<T>(T left) where T : IBinaryInteger<T>, IAdditionOperators<T, T, T> {
		public T DivideByRoundUp(T right) => (left - T.One) / right + T.One;

		public T GreatestCommonDivisor(T right) {
			while (right != T.Zero) {
				var temp = right;
				right = left % right;
				left = temp;
			}

			return left;
		}

		public T FirstMultipleOf(T multiple) {
			var gcd = left.GreatestCommonDivisor(multiple);
			var k = multiple / gcd;
			return left * k;
		}
	}

	public static T Clamp<T, TValue>(this TValue value) where T : struct, INumberBase<T> where TValue : struct, INumberBase<TValue> => T.CreateSaturating(value);
}
