namespace DragonLib.Hash.Basis;

// https://reveng.sourceforge.io/crc-catalogue/all.htm
public static class CRC16Variants {
	public static CRCVariant<ushort> Default => USB;

	public static CRCVariant<ushort> ARC { get; } = new(0x8005, 0x0000, 0x0000, true, true);
	public static CRCVariant<ushort> CDMA { get; } = new(0xc867, ushort.MaxValue, 0x0000, false, false);
	public static CRCVariant<ushort> CMS { get; } = new(0x8005, ushort.MaxValue, 0x0000, false, false);
	public static CRCVariant<ushort> DDS { get; } = new(0x8005, 0x800d, 0x0000, false, false);
	public static CRCVariant<ushort> DECT1 { get; } = new(0x0589, 0x0000, 0x0001, false, false);
	public static CRCVariant<ushort> DECT { get; } = new(0x0589, 0x0000, 0x0000, false, false);
	public static CRCVariant<ushort> DNP { get; } = new(0x3d65, 0x0000, ushort.MaxValue, true, true);
	public static CRCVariant<ushort> EN { get; } = new(0x3d65, 0x0000, ushort.MaxValue, false, false);
	public static CRCVariant<ushort> GENIbus { get; } = new(0x1021, ushort.MaxValue, ushort.MaxValue, false, false);
	public static CRCVariant<ushort> GSM { get; } = new(0x1021, 0x0000, ushort.MaxValue, false, false);
	public static CRCVariant<ushort> IBM { get; } = new(0x1021, ushort.MaxValue, 0x0000, false, false);
	public static CRCVariant<ushort> SDLC { get; } = new(0x1021, ushort.MaxValue, ushort.MaxValue, true, true);
	public static CRCVariant<ushort> Kermit { get; } = new(0x1021, 0x0000, 0x0000, true, true);
	public static CRCVariant<ushort> LJ { get; } = new(0x6f63, 0x0000, 0x0000, false, false);
	public static CRCVariant<ushort> MAXIM { get; } = new(0x8005, 0x0000, ushort.MaxValue, true, true);
	public static CRCVariant<ushort> MCRF { get; } = new(0x1021, ushort.MaxValue, 0x0000, true, true);
	public static CRCVariant<ushort> Modbus { get; } = new(0x8005, ushort.MaxValue, 0x0000, true, true);
	public static CRCVariant<ushort> NRSC { get; } = new(0x080b, ushort.MaxValue, 0x0000, true, true);
	public static CRCVariant<ushort> OpenSAFETY { get; } = new(0x5935, 0x0000, 0x0000, false, false);
	public static CRCVariant<ushort> OpenSAFETYB { get; } = new(0x755b, 0x0000, 0x0000, false, false);
	public static CRCVariant<ushort> ProfiBus { get; } = new(0x1dcf, ushort.MaxValue, ushort.MaxValue, false, false);
	public static CRCVariant<ushort> Fujitsu { get; } = new(0x1021, 0x1d0f, 0x0000, false, false);
	public static CRCVariant<ushort> DIF { get; } = new(0x8bb7, 0x0000, 0x0000, false, false);
	public static CRCVariant<ushort> Teledisk { get; } = new(0xa097, 0x0000, 0x0000, false, false);
	public static CRCVariant<ushort> UMTS { get; } = new(0x8005, 0x0000, 0x0000, false, false);
	public static CRCVariant<ushort> USB { get; } = new(0x8005, ushort.MaxValue, ushort.MaxValue, true, true);
	public static CRCVariant<ushort> XMODEM { get; } = new(0x1021, 0x0000, 0x0000, false, false);
}
