using DragonLib.Hash;
using DragonLib.Hash.Algorithms;
using DragonLib.Hash.Basis;

namespace DragonLib.Tests.Hash;

public class CRCTests {
	[Test]
	public void SimdTest() {
		if (!CRC32CAlgorithm.IsSupported) {
			Assert.Inconclusive("No SSE4.2 instruction set");
			return;
		}

		using var simd = new CRC32CAlgorithm();
		var test = simd.ComputeHash("123456789"u8.ToArray());
		Assert.That(test, Is.EqualTo(BitConverter.GetBytes(0xe3069283)));
		var test2 = simd.ComputeHashValue("123456789"u8.ToArray());
		Assert.That(test2, Is.EqualTo(0xe3069283));
	}

	[TestCase("ISO", 0xb90956c775a41001ul)] [TestCase("ECMA182", 0x6c40df5f0b497347ul)] [TestCase("Wolfgang", 0x62ec59e3f1a4f00aul)] [TestCase("ECMA", 0x995dc9bbdf1939faul)] [TestCase("Microsoft", 0x75d4b74f024eceeaul)] [TestCase("PyJones", 0xcaa717168609f281)] [TestCase("Jones", 0xe9c6d914c4b8d9ca)]
	public void CRC64Test(string name, ulong check) {
		var variant = (CRCVariant<ulong>) typeof(CRC64Variants).GetProperty(name)!.GetValue(null)!;

		using var crc = CRC.Create(variant);
		var test = crc.ComputeHashValue("123456789"u8.ToArray());
		Assert.That(test, Is.EqualTo(check));
	}

	[TestCase("Castagnoli", 0xe3069283u)] [TestCase("AIXM", 0x3010bf7fu)] [TestCase("AUTOSAR", 0x1697d06au)] [TestCase("BASE91", 0x87315576u)] [TestCase("BZip2", 0xfc891918u)] [TestCase("CDROM", 0x6ec2edc4u)] [TestCase("POSIX", 0x765e7680u)] [TestCase("ISO", 0xcbf43926u)] [TestCase("JAM", 0x340bc6d9u)] [TestCase("MEF", 0xd2c22f51u)] [TestCase("MPEG2", 0x0376e6e7u)] [TestCase("XFER", 0xbd0be338u)]
	public void CRC32Test(string name, uint check) {
		var variant = (CRCVariant<uint>) typeof(CRC32Variants).GetProperty(name)!.GetValue(null)!;

		using var crc = CRC.Create(variant);
		var test = crc.ComputeHashValue("123456789"u8.ToArray());
		Assert.That(test, Is.EqualTo(check));
	}

	[TestCase("DVB", 0xbc)] [TestCase("AUTOSAR", 0xdf)] [TestCase("Bluetooth", 0x26)] [TestCase("CDMA", 0xda)] [TestCase("DARC", 0x15)] [TestCase("FFmpeg", 0x37)] [TestCase("GSM", 0x94)] [TestCase("HITAG", 0xb4)] [TestCase("ITU", 0xa1)] [TestCase("CODE", 0x7e)] [TestCase("LTE", 0xea)] [TestCase("MAXIM", 0xa1)] [TestCase("MIFARE", 0x99)] [TestCase("NRSC", 0xf7)] [TestCase("OpenSAFETY", 0x3e)] [TestCase("ROHC", 0xd0)] [TestCase("SAE", 0x4b)] [TestCase("SMBus", 0xf4)] [TestCase("Tech", 0x97)]
	[TestCase("WCDMA", 0x25)]
	public void CRC8Test(string name, byte check) {
		var variant = (CRCVariant<byte>) typeof(CRC8Variants).GetProperty(name)!.GetValue(null)!;

		using var crc = CRC.Create(variant);
		var test = crc.ComputeHashValue("123456789"u8.ToArray());
		Assert.That(test, Is.EqualTo(check));
	}

	[TestCase("ARC", 0xbb3du)] [TestCase("CDMA", 0x4c06u)] [TestCase("CMS", 0xaee7u)] [TestCase("DDS", 0x9ecfu)] [TestCase("DECT1", 0x007eu)] [TestCase("DECT", 0x007fu)] [TestCase("DNP", 0xea82u)] [TestCase("EN", 0xc2b7u)] [TestCase("GENIbus", 0xd64eu)] [TestCase("GSM", 0xce3cu)] [TestCase("IBM", 0x29b1u)] [TestCase("SDLC", 0x906eu)] [TestCase("Kermit", 0x2189u)] [TestCase("LJ", 0xbdf4u)] [TestCase("MAXIM", 0x44c2u)] [TestCase("MCRF", 0x6f91u)] [TestCase("Modbus", 0x4b37u)]
	[TestCase("NRSC", 0xa066u)] [TestCase("OpenSAFETY", 0x5d38u)] [TestCase("OpenSAFETYB", 0x20feu)] [TestCase("ProfiBus", 0xa819u)] [TestCase("Fujitsu", 0xe5ccu)] [TestCase("DIF", 0xd0dbu)] [TestCase("Teledisk", 0x0fb3u)] [TestCase("UMTS", 0xfee8u)] [TestCase("USB", 0xb4c8u)] [TestCase("XMODEM", 0x31c3u)]
	public void CRC16Test(string name, uint check) {
		var variant = (CRCVariant<ushort>) typeof(CRC16Variants).GetProperty(name)!.GetValue(null)!;

		using var crc = CRC.Create(variant);
		var test = crc.ComputeHashValue("123456789"u8.ToArray());
		Assert.That(test, Is.EqualTo((ushort) check));
	}
}
