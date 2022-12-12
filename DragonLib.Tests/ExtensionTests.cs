using System.Text;

namespace DragonLib.Tests;

public class Extensions {
    [Test]
    public void SpanClone() {
        Span<byte> testSpan = stackalloc byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var clone = testSpan.Clone();
        Assert.That(clone[0], Is.EqualTo(testSpan[0]));
        clone[0] = 2;
        Assert.That(clone[0], Is.Not.EqualTo(testSpan[0]));
        Assert.That(testSpan[0], Is.EqualTo(1));
    }

    [Test]
    public void SanitizeFilenameValid() {
        const string sample = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.";
        var sanitized = sample.SanitizeFilename();
        Assert.That(sanitized, Is.EqualTo(sample));
    }

    [Test]
    public void SanitizeFilenameInvalid() {
        const string sample = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.";
        var invalid = Path.GetInvalidFileNameChars();
        foreach (var c in invalid) {
            var sanitized = (sample + c).SanitizeFilename();
            Assert.That(sanitized, Is.Not.EqualTo(sample));
            Assert.That(sanitized.ToCharArray(), Does.Not.Contain(c));
            Assert.That(sanitized[^1], Is.EqualTo('_'));
        }
    }

    [Test]
    public void SanitizeFilenameInvalidCustom() {
        const string sample = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.";
        var invalid = Path.GetInvalidFileNameChars();
        foreach (var c in invalid) {
            var sanitized = (sample + c).SanitizeFilename('.');
            Assert.That(sanitized, Is.Not.EqualTo(sample));
            Assert.That(sanitized.ToCharArray(), Does.Not.Contain(c));
            Assert.That(sanitized[^1], Is.EqualTo('.'));
        }
    }

    [Test]
    public void SanitizeDirnameValidWin() {
        const string sample = "C:\\Test\\Path.bin";
        var sanitized = sample.SanitizeDirname();
        Assert.That(sanitized, Is.EqualTo(sample));
    }

    [Test]
    public void SanitizeDirnameValidUnix() {
        const string sample = "/Test/Path.bin";
        var sanitized = sample.SanitizeDirname();
        Assert.That(sanitized, Is.EqualTo(sample));
    }

    [Test]
    public void SanitizeDirnameInvalidWin() {
        const string sample = "C:\\Test\\Path.bin";
        var invalid = Path.GetInvalidPathChars();
        foreach (var c in invalid) {
            var sanitized = (sample + c).SanitizeDirname();
            Assert.That(sanitized, Is.Not.EqualTo(sample));
            Assert.That(sanitized.ToCharArray(), Does.Not.Contain(c));
            Assert.That(sanitized[^1], Is.EqualTo('_'));
        }
    }

    [Test]
    public void SanitizeDirnameInvalidUnix() {
        const string sample = "/Test/Path.bin";
        var invalid = Path.GetInvalidPathChars();
        foreach (var c in invalid) {
            var sanitized = (sample + c).SanitizeDirname();
            Assert.That(sanitized, Is.Not.EqualTo(sample));
            Assert.That(sanitized.ToCharArray(), Does.Not.Contain(c));
            Assert.That(sanitized[^1], Is.EqualTo('_'));
        }
    }

    [Test]
    public void SanitizeDirnameInvalidCustomWin() {
        const string sample = "C:\\Test\\Path.bin";
        var invalid = Path.GetInvalidPathChars();
        foreach (var c in invalid) {
            var sanitized = (sample + c).SanitizeDirname('.');
            Assert.That(sanitized, Is.Not.EqualTo(sample));
            Assert.That(sanitized.ToCharArray(), Does.Not.Contain(c));
            Assert.That(sanitized[^1], Is.EqualTo('.'));
        }
    }

    [Test]
    public void SanitizeDirnameInvalidCustomUnix() {
        const string sample = "/Test/Path.bin";
        var invalid = Path.GetInvalidPathChars();
        foreach (var c in invalid) {
            var sanitized = (sample + c).SanitizeDirname('.');
            Assert.That(sanitized, Is.Not.EqualTo(sample));
            Assert.That(sanitized.ToCharArray(), Does.Not.Contain(c));
            Assert.That(sanitized[^1], Is.EqualTo('.'));
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
        Span<uint> test = stackalloc uint[1] { 0x12345678 };
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
        ReadOnlySpan<uint> test = stackalloc uint[1] { 0x12345678 };
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
        var str = test.ReadUTF8String();
        Assert.That(str, Is.EqualTo("ABCDE"));
    }

    [Test]
    public void ReadUTF8StringLimit() {
        Span<byte> test = "ABCDE\0FGHIJ"u8.ToArray();
        var str = test.ReadUTF8String(3);
        Assert.That(str, Is.EqualTo("ABC"));
    }

    [Test]
    public void ReadUTF8StringNull() {
        Span<byte> test = "\0"u8.ToArray();
        var str = test.ReadUTF8String();
        Assert.That(str, Is.Null);
    }

    [Test]
    public void ReadUTF8StringNonNull() {
        Span<byte> test = "\0"u8.ToArray();
        var str = test.ReadUTF8StringNonNull();
        Assert.That(str, Is.Empty);
    }

    [Test]
    public void ReadUTF16String() {
        var test = Encoding.Unicode.GetBytes("ABCDE\0FGHIJ").AsSpan();
        var str = test.ReadUTF16String();
        Assert.That(str, Is.EqualTo("ABCDE"));
    }

    [Test]
    public void ReadUTF16StringLimit() {
        var test = Encoding.Unicode.GetBytes("ABCDE\0FGHIJ").AsSpan();
        var str = test.ReadUTF16String(3);
        Assert.That(str, Is.EqualTo("ABC"));
    }

    [Test]
    public void ReadUTF16StringNull() {
        var test = Encoding.Unicode.GetBytes("\0").AsSpan();
        var str = test.ReadUTF16String();
        Assert.That(str, Is.Null);
    }

    [Test]
    public void ReadUTF16StringNonNull() {
        var test = Encoding.Unicode.GetBytes("\0").AsSpan();
        var str = test.ReadUTF16StringNonNull();
        Assert.That(str, Is.Empty);
    }

    [Test]
    public void ReadUTF16StringBE() {
        var test = Encoding.BigEndianUnicode.GetBytes("ABCDE\0FGHIJ").AsSpan();
        var str = test.ReadUTF16String(bigEndian: true);
        Assert.That(str, Is.EqualTo("ABCDE"));
    }

    [Test]
    public void ReadUTF16StringLimitBE() {
        var test = Encoding.BigEndianUnicode.GetBytes("ABCDE\0FGHIJ").AsSpan();
        var str = test.ReadUTF16String(3, true);
        Assert.That(str, Is.EqualTo("ABC"));
    }

    [Test]
    public void ReadUTF16StringBENull() {
        var test = Encoding.BigEndianUnicode.GetBytes("\0").AsSpan();
        var str = test.ReadUTF16String(bigEndian: true);
        Assert.That(str, Is.Null);
    }

    [Test]
    public void ReadUTF16StringBENonNull() {
        var test = Encoding.BigEndianUnicode.GetBytes("\0").AsSpan();
        var str = test.ReadUTF16StringNonNull(bigEndian: true);
        Assert.That(str, Is.Empty);
    }

    [Test]
    public void ReadUTF32String() {
        var test = Encoding.UTF32.GetBytes("ABCDE\0FGHIJ").AsSpan();
        var str = test.ReadUTF32String();
        Assert.That(str, Is.EqualTo("ABCDE"));
    }

    [Test]
    public void ReadUTF32StringLimit() {
        var test = Encoding.UTF32.GetBytes("ABCDE\0FGHIJ").AsSpan();
        var str = test.ReadUTF32String(3);
        Assert.That(str, Is.EqualTo("ABC"));
    }

    [Test]
    public void ReadUTF32StringNull() {
        var test = Encoding.UTF32.GetBytes("\0").AsSpan();
        var str = test.ReadUTF32String();
        Assert.That(str, Is.Null);
    }

    [Test]
    public void ReadUTF32StringNonNull() {
        var test = Encoding.UTF32.GetBytes("\0").AsSpan();
        var str = test.ReadUTF32StringNonNull();
        Assert.That(str, Is.Empty);
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
    public void ToHexOctetsInvalid() {
        Assert.Throws<FormatException>(() => "00 11 22 3".ToHexOctets());
    }

    [Test]
    public void ToHexString() {
        Span<byte> input = stackalloc byte[] { 0x00, 0x11, 0x22, 0x33 };
        var output = input.ToHexString();
        Assert.That(output, Is.EqualTo("00112233"));
    }

    [Test]
    public void ToHexStringSeparator() {
        Span<byte> input = stackalloc byte[] { 0x00, 0x11, 0x22, 0x33 };
        var output = input.ToHexString(" ");
        Assert.That(output, Is.EqualTo("00 11 22 33"));
    }

    [Test]
    public void ToHexStringPrefix() {
        Span<byte> input = stackalloc byte[] { 0x00, 0x11, 0x22, 0x33 };
        var output = input.ToHexString(prefix: "0x");
        Assert.That(output, Is.EqualTo("0x00112233"));
    }

    [Test]
    public void ToHexStringSeparatorAndPrefix() {
        Span<byte> input = stackalloc byte[] { 0x00, 0x11, 0x22, 0x33 };
        var output = input.ToHexString(" ", "0x");
        Assert.That(output, Is.EqualTo("0x00 0x11 0x22 0x33"));
    }

    [Test]
    public void ToHexString32() {
        Span<uint> input = stackalloc uint[] { 0x00112233, 0x44556677 };
        var output = input.ToHexString();
        Assert.That(output, Is.EqualTo("0011223344556677"));
    }

    [Test]
    public void ToHexString32Separator() {
        Span<uint> input = stackalloc uint[] { 0x00112233, 0x44556677 };
        var output = input.ToHexString(" ");
        Assert.That(output, Is.EqualTo("00112233 44556677"));
    }

    [Test]
    public void ToHexString32Prefix() {
        Span<uint> input = stackalloc uint[] { 0x00112233, 0x44556677 };
        var output = input.ToHexString(prefix: "0x");
        Assert.That(output, Is.EqualTo("0x0011223344556677"));
    }

    [Test]
    public void ToHexString32SeparatorAndPrefix() {
        Span<uint> input = stackalloc uint[] { 0x00112233, 0x44556677 };
        var output = input.ToHexString(" ", "0x");
        Assert.That(output, Is.EqualTo("0x00112233 0x44556677"));
    }

    [Test]
    public void ToBytes() {
        var output = "00112233".ToBytes();
        Assert.That(output, Is.EqualTo(new byte[] { 0x00, 0x11, 0x22, 0x33 }));
    }

    [Test]
    public void ToBytesOctet() {
        var output = "01234567".ToBytes(1);
        Assert.That(output, Is.EqualTo(new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7 }));
    }

    [Test]
    public void ToBytesEmpty() {
        var output = string.Empty.ToBytes();
        Assert.That(output, Is.Empty);
    }

    [Test]
    public void ToBytesNull() {
        string? input = null;
        var output = input.ToBytes();
        Assert.That(output, Is.Empty);
    }

    [Test]
    public void ToBytesInvalid1() {
        Assert.Throws<FormatException>(() => "00 11 22 3".ToBytes());
    }

    [Test]
    public void ToBytesInvalid2() {
        Assert.Throws<FormatException>(() => "00 11 22".ToBytes(0));
    }

    [Test]
    public void ToBytesInvalid3() {
        Assert.Throws<FormatException>(() => "00 11 22".ToBytes(3));
    }

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
        const byte b = 0xab;
        Assert.That(b.GetHighNibble(), Is.EqualTo(0xa));
    }

    [Test]
    public void GetLowNibble() {
        const byte b = 0xab;
        Assert.That(b.GetLowNibble(), Is.EqualTo(0xb));
    }

    [Test]
    public void GetSignedHighNibble() {
        const byte b = 0xab;
        Assert.That(b.GetHighNibbleSigned(), Is.EqualTo(-0x6));
    }

    [Test]
    public void GetSignedLowNibble() {
        const byte b = 0xab;
        Assert.That(b.GetLowNibbleSigned(), Is.EqualTo(-0x5));
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
