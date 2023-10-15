namespace DragonLib.Hash.Basis;

// https://reveng.sourceforge.io/crc-catalogue/all.htm
public static class CRC8Variants {
    public static CRCVariant<byte> Default => DVB;

    public static CRCVariant<byte> DVB { get; } = new(0xd5, 0, 0, false, false);
    public static CRCVariant<byte> AUTOSAR { get; } = new(0x2f, byte.MaxValue, byte.MaxValue, false, false);
    public static CRCVariant<byte> Bluetooth { get; } = new(0xa7, 0, 0, true, true);
    public static CRCVariant<byte> CDMA { get; } = new(0x9b, byte.MaxValue, 0, false, false);
    public static CRCVariant<byte> DARC { get; } = new(0x39, 0, 0, true, true);
    public static CRCVariant<byte> FFmpeg { get; } = new(0x1d, 0, 0, false, false);
    public static CRCVariant<byte> GSM { get; } = new(0x49, 0, byte.MaxValue, false, false);
    public static CRCVariant<byte> HITAG { get; } = new(0x1d, byte.MaxValue, 0, false, false);
    public static CRCVariant<byte> ITU { get; } = new(0x07, 0, 0x55, false, false);
    public static CRCVariant<byte> CODE { get; } = new(0x1d, 0xfd, 0, false, false);
    public static CRCVariant<byte> LTE { get; } = new(0x9b, 0, 0, false, false);
    public static CRCVariant<byte> MAXIM { get; } = new(0x31, 0, 0, true, true);
    public static CRCVariant<byte> MIFARE { get; } = new(0x1d, 0xc7, 0, false, false);
    public static CRCVariant<byte> NRSC { get; } = new(0x31, byte.MaxValue, 0, false, false);
    public static CRCVariant<byte> OpenSAFETY { get; } = new(0x2f, 0, 0, false, false);
    public static CRCVariant<byte> ROHC { get; } = new(0x07, byte.MaxValue, 0, true, true);
    public static CRCVariant<byte> SAE { get; } = new(0x1d, byte.MaxValue, byte.MaxValue, false, false);
    public static CRCVariant<byte> SMBus { get; } = new(0x07, 0, 0, false, false);
    public static CRCVariant<byte> Tech { get; } = new(0x1d, byte.MaxValue, 0, true, true);
    public static CRCVariant<byte> WCDMA { get; } = new(0x9b, 0, 0, true, true);
}
