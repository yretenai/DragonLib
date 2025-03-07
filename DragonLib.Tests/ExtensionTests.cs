using System.Text;

namespace DragonLib.Tests;

public class ExtensionTests {
	[Test]
	public void SpanClone() {
		Span<byte> testSpan = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
		var clone = testSpan.Clone();
		Assert.That(clone[0], Is.EqualTo(testSpan[0]));
		clone[0] = 2;
	#pragma warning disable NUnit2045
		Assert.That(clone[0], Is.Not.EqualTo(testSpan[0]));
		Assert.That(testSpan[0], Is.EqualTo(1));
	#pragma warning restore NUnit2045
	}

	[Test]
	public void SanitizeFilenameValid() {
		const string SAMPLE = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.";
		var sanitized = SAMPLE.SanitizeFilename();
		Assert.That(sanitized, Is.EqualTo(SAMPLE));
	}

	[Test]
	public void SanitizeFilenameInvalid() {
		const string SAMPLE = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.";
		var invalid = Path.GetInvalidFileNameChars();
		foreach (var c in invalid) {
			var sanitized = (SAMPLE + c).SanitizeFilename();
			Assert.Multiple(() => {
				Assert.That(sanitized, Is.Not.EqualTo(SAMPLE));
				Assert.That(sanitized.ToCharArray(), Does.Not.Contain(c));
				Assert.That(sanitized[^1], Is.EqualTo('_'));
			});
		}
	}

	[Test]
	public void SanitizeFilenameInvalidCustom() {
		const string SAMPLE = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.";
		var invalid = Path.GetInvalidFileNameChars();
		foreach (var c in invalid) {
			var sanitized = (SAMPLE + c).SanitizeFilename('.');
			Assert.Multiple(() => {
				Assert.That(sanitized, Is.Not.EqualTo(SAMPLE));
				Assert.That(sanitized.ToCharArray(), Does.Not.Contain(c));
				Assert.That(sanitized[^1], Is.EqualTo('.'));
			});
		}
	}

	[Test]
	public void SanitizeDirnameValidWin() {
		const string SAMPLE = @"C:\Test\Path.bin";
		var sanitized = SAMPLE.SanitizeDirname();
		Assert.That(sanitized, Is.EqualTo(SAMPLE));
	}

	[Test]
	public void SanitizeDirnameValidUnix() {
		const string SAMPLE = "/Test/Path.bin";
		var sanitized = SAMPLE.SanitizeDirname();
		Assert.That(sanitized, Is.EqualTo(SAMPLE));
	}

	[Test]
	public void SanitizeDirnameInvalidWin() {
		const string SAMPLE = @"C:\Test\Path.bin";
		var invalid = Path.GetInvalidPathChars();
		foreach (var c in invalid) {
			var sanitized = (SAMPLE + c).SanitizeDirname();
			Assert.Multiple(() => {
				Assert.That(sanitized, Is.Not.EqualTo(SAMPLE));
				Assert.That(sanitized.ToCharArray(), Does.Not.Contain(c));
				Assert.That(sanitized[^1], Is.EqualTo('_'));
			});
		}
	}

	[Test]
	public void SanitizeDirnameInvalidUnix() {
		const string SAMPLE = "/Test/Path.bin";
		var invalid = Path.GetInvalidPathChars();
		foreach (var c in invalid) {
			var sanitized = (SAMPLE + c).SanitizeDirname();
			Assert.Multiple(() => {
				Assert.That(sanitized, Is.Not.EqualTo(SAMPLE));
				Assert.That(sanitized.ToCharArray(), Does.Not.Contain(c));
				Assert.That(sanitized[^1], Is.EqualTo('_'));
			});
		}
	}

	[Test]
	public void SanitizeDirnameInvalidCustomWin() {
		const string SAMPLE = @"C:\Test\Path.bin";
		var invalid = Path.GetInvalidPathChars();
		foreach (var c in invalid) {
			var sanitized = (SAMPLE + c).SanitizeDirname('.');
			Assert.Multiple(() => {
				Assert.That(sanitized, Is.Not.EqualTo(SAMPLE));
				Assert.That(sanitized.ToCharArray(), Does.Not.Contain(c));
				Assert.That(sanitized[^1], Is.EqualTo('.'));
			});
		}
	}

	[Test]
	public void SanitizeDirnameInvalidCustomUnix() {
		const string SAMPLE = "/Test/Path.bin";
		var invalid = Path.GetInvalidPathChars();
		foreach (var c in invalid) {
			var sanitized = (SAMPLE + c).SanitizeDirname('.');
			Assert.Multiple(() => {
				Assert.That(sanitized, Is.Not.EqualTo(SAMPLE));
				Assert.That(sanitized.ToCharArray(), Does.Not.Contain(c));
				Assert.That(sanitized[^1], Is.EqualTo('.'));
			});
		}
	}

	[Test]
	public void EnsureThatDirectoryExists() {
		var path = Path.Combine(Path.GetTempPath(), "DragonLibTest") + Path.DirectorySeparatorChar;
		if (Directory.Exists(path)) {
			Assert.Inconclusive();
			return;
		}

		path.EnsureDirectoryExists();
		Assert.That(Directory.Exists(path));

		Directory.Delete(path);
	}

	[Test]
	public void EnsureThatDirectoriesExist() {
		var path1 = Path.Combine(Path.GetTempPath(), "DragonLibTest1") + Path.DirectorySeparatorChar;
		if (Directory.Exists(path1)) {
			Assert.Inconclusive();
			return;
		}

		var path2 = Path.Combine(Path.GetTempPath(), "DragonLibTest2") + Path.DirectorySeparatorChar;
		if (Directory.Exists(path1)) {
			Assert.Inconclusive();
			return;
		}

		new[] { path1, path2 }.EnsureDirectoriesExists();
		Assert.That(Directory.Exists(path1));
		Assert.That(Directory.Exists(path2));

		Directory.Delete(path1);
		Directory.Delete(path2);
	}

	[Test]
	public void AsBytes() {
		Span<uint> test = [0x12345678];
		var bytes = test.AsBytes();
		Assert.That(bytes.Length, Is.EqualTo(4));
		Assert.That(bytes[0], Is.EqualTo(0x78));
		Assert.That(bytes[1], Is.EqualTo(0x56));
		Assert.That(bytes[2], Is.EqualTo(0x34));
		Assert.That(bytes[3], Is.EqualTo(0x12));

		bytes[0] = 0;
		bytes[1] = 0;
		bytes[2] = 0;
		bytes[3] = 0;

		Assert.That(test[0], Is.EqualTo(0));
	}

	[Test]
	public void ReadOnlyAsBytes() {
		ReadOnlySpan<uint> test = [0x12345678];
		var bytes = test.AsBytes();
		Assert.That(bytes.Length, Is.EqualTo(4));
		Assert.That(bytes[0], Is.EqualTo(0x78));
		Assert.That(bytes[1], Is.EqualTo(0x56));
		Assert.That(bytes[2], Is.EqualTo(0x34));
		Assert.That(bytes[3], Is.EqualTo(0x12));
	}

	[Test]
	public void ReadUTF8String() {
		Span<byte> test = "ABCDE\0FGHIJ"u8.ToArray();
		var str = test.ReadString(Encoding.UTF8);
		Assert.That(str, Is.EqualTo("ABCDE"));
	}

	[Test]
	public void ReadUTF8StringLimit() {
		Span<byte> test = "ABCDE\0FGHIJ"u8.ToArray();
		var str = test.ReadString(Encoding.UTF8, 3);
		Assert.That(str, Is.EqualTo("ABC"));
	}

	[Test]
	public void ReadUTF8StringNull() {
		Span<byte> test = "\0"u8.ToArray();
		var str = test.ReadString(Encoding.UTF8);
		Assert.That(str, Is.Null);
	}

	[Test]
	public void ReadUTF16String() {
		var test = Encoding.Unicode.GetBytes("ABCDE\0FGHIJ").AsSpan();
		var str = test.As<ushort>().ReadString(Encoding.Unicode);
		Assert.That(str, Is.EqualTo("ABCDE"));
	}

	[Test]
	public void ReadUTF16StringLimit() {
		var test = Encoding.Unicode.GetBytes("ABCDE\0FGHIJ").AsSpan();
		var str = test.As<ushort>().ReadString(Encoding.Unicode, 3);
		Assert.That(str, Is.EqualTo("ABC"));
	}

	[Test]
	public void ReadUTF16StringNull() {
		var test = Encoding.Unicode.GetBytes("\0").AsSpan();
		var str = test.As<ushort>().ReadString(Encoding.Unicode);
		Assert.That(str, Is.Null);
	}

	[Test]
	public void ReadUTF16StringBE() {
		var test = Encoding.BigEndianUnicode.GetBytes("ABCDE\0FGHIJ").AsSpan();
		var str = test.As<ushort>().ReadString(Encoding.BigEndianUnicode);
		Assert.That(str, Is.EqualTo("ABCDE"));
	}

	[Test]
	public void ReadUTF16StringLimitBE() {
		var test = Encoding.BigEndianUnicode.GetBytes("ABCDE\0FGHIJ").AsSpan();
		var str = test.As<ushort>().ReadString(Encoding.BigEndianUnicode, 3);
		Assert.That(str, Is.EqualTo("ABC"));
	}

	[Test]
	public void ReadUTF16StringBENull() {
		var test = Encoding.BigEndianUnicode.GetBytes("\0").AsSpan();
		var str = test.As<ushort>().ReadString(Encoding.BigEndianUnicode);
		Assert.That(str, Is.Null);
	}

	[Test]
	public void ReadUTF32String() {
		var test = Encoding.UTF32.GetBytes("ABCDE\0FGHIJ").AsSpan();
		var str = test.As<uint>().ReadString(Encoding.UTF32);
		Assert.That(str, Is.EqualTo("ABCDE"));
	}

	[Test]
	public void ReadUTF32StringLimit() {
		var test = Encoding.UTF32.GetBytes("ABCDE\0FGHIJ").AsSpan();
		var str = test.As<uint>().ReadString(Encoding.UTF32, 3);
		Assert.That(str, Is.EqualTo("ABC"));
	}

	[Test]
	public void ReadUTF32StringNull() {
		var test = Encoding.UTF32.GetBytes("\0").AsSpan();
		var str = test.As<uint>().ReadString(Encoding.UTF32);
		Assert.That(str, Is.Null);
	}

	[Test]
	public void AlignIntExact() {
		var n = 4.Align(4);
		Assert.That(n, Is.EqualTo(4));
	}

	[Test]
	public void AlignInt() {
		var n = 2.Align(4);
		Assert.That(n, Is.EqualTo(4));
	}

	[Test]
	public void AlignUIntExact() {
		var n = 4u.Align(4);
		Assert.That(n, Is.EqualTo(4));
	}

	[Test]
	public void AlignUInt() {
		var n = 2u.Align(4);
		Assert.That(n, Is.EqualTo(4));
	}

	[Test]
	public void AlignLongExact() {
		var n = 4L.Align(4);
		Assert.That(n, Is.EqualTo(4));
	}

	[Test]
	public void AlignLong() {
		var n = 2L.Align(4);
		Assert.That(n, Is.EqualTo(4));
	}

	[Test]
	public void AlignULongExact() {
		var n = 4ul.Align(4);
		Assert.That(n, Is.EqualTo(4));
	}

	[Test]
	public void AlignULong() {
		var n = 2ul.Align(4);
		Assert.That(n, Is.EqualTo(4));
	}

	[Test]
	public void UnixPath() {
		var path = "test\\path".UnixPath(false);
		Assert.That(path, Is.EqualTo("test/path"));
	}

	[Test]
	public void UnixPathDir() {
		var path = "test\\path".UnixPath(true);
		Assert.That(path, Is.EqualTo("test/path/"));
	}

	[Test]
	public void UnixPathEmpty() {
		var path = "".UnixPath(false);
		Assert.That(path, Is.EqualTo(""));
	}

	[Test]
	public void UnixPathEmptyDir() {
		var path = "".UnixPath(true);
		Assert.That(path, Is.EqualTo(""));
	}

	[Test]
	public void ToHexOctets() {
		var input = "00 11 22 33";
		var output = input.ToHexOctets();
		Assert.That(output, Is.EqualTo(new[] { "00", "11", "22", "33" }));
	}

	[Test]
	public void ToHexOctetsEmpty() {
		var output = string.Empty.ToHexOctets();
		Assert.That(output, Is.Empty);
	}

	[Test]
	public void ToHexOctetsNull() {
		string? input = null;
		var output = input.ToHexOctets();
		Assert.That(output, Is.Empty);
	}

	[Test]
	public void ToHexOctetsInvalid() => Assert.Throws<FormatException>(() => "00 11 22 3".ToHexOctets());

	[Test]
	public void DivideByRoundUp() {
		var n = 5.DivideByRoundUp(2);
		Assert.That(n, Is.EqualTo(3));
	}

	[Test]
	public void DivideByRoundUpExact() {
		var n = 6.DivideByRoundUp(2);
		Assert.That(n, Is.EqualTo(3));
	}

	[Test]
	public void DivideByRoundUpZero() {
		var n = 0.DivideByRoundUp(2);
		Assert.That(n, Is.EqualTo(0));
	}

	[Test]
	public void ShortMaxClamp() {
		var n = 0xFFFFF.Clamp<short, int>();
		Assert.That(n, Is.EqualTo(short.MaxValue));
	}

	[Test]
	public void ShortMinClamp() {
		var n = (-0xFFFFF).Clamp<short, int>();
		Assert.That(n, Is.EqualTo(short.MinValue));
	}

	[Test]
	public void ShortEvenClamp() {
		var n = 0.Clamp<short, int>();
		Assert.That(n, Is.EqualTo(0));
	}

	[Test]
	public void GetHighNibble() {
		const byte TEST = 0xab;
		Assert.That(TEST.GetHighNibble(), Is.EqualTo(0xa));
	}

	[Test]
	public void GetLowNibble() {
		const byte TEST = 0xab;
		Assert.That(TEST.GetLowNibble(), Is.EqualTo(0xb));
	}

	[Test]
	public void GetSignedHighNibble() {
		const byte TEST = 0xab;
		Assert.That(TEST.GetHighNibbleSigned(), Is.EqualTo(-0x6));
	}

	[Test]
	public void GetSignedLowNibble() {
		const byte TEST = 0xab;
		Assert.That(TEST.GetLowNibbleSigned(), Is.EqualTo(-0x5));
	}

	[Test]
	public void GetHumanReadableBytesB() {
		var amount = 1L.GetHumanReadableBytes();
		Assert.That(amount, Is.EqualTo("1 B"));
	}

	[Test]
	public void GetHumanReadableBytesKB() {
		var amount = 0x400UL.GetHumanReadableBytes();
		Assert.That(amount, Is.EqualTo("1 KiB"));
	}

	[Test]
	public void GetHumanReadableBytesMB() {
		var amount = 0x100000UL.GetHumanReadableBytes();
		Assert.That(amount, Is.EqualTo("1 MiB"));
	}

	[Test]
	public void GetHumanReadableBytesGB() {
		var amount = 0x40000000UL.GetHumanReadableBytes();
		Assert.That(amount, Is.EqualTo("1 GiB"));
	}

	[Test]
	public void GetHumanReadableBytesTB() {
		var amount = 0x10000000000UL.GetHumanReadableBytes();
		Assert.That(amount, Is.EqualTo("1 TiB"));
	}

	[Test]
	public void GetHumanReadableBytesPB() {
		var amount = 0x4000000000000UL.GetHumanReadableBytes();
		Assert.That(amount, Is.EqualTo("1 PiB"));
	}

	[Test]
	public void GetHumanReadableBytesEB() {
		var amount = 0x1000000000000000UL.GetHumanReadableBytes();
		Assert.That(amount, Is.EqualTo("1 EiB"));
	}
}
