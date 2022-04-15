namespace DragonLib.Hash.Basis;

// https://reveng.sourceforge.io/crc-catalogue/all.htm
public static class CRC32Variants {
    public static CRCVariant<uint> Default => Castagnoli; // ISO is CRC-32, but Castagnoli is more popular due to Intel.
    public static CRCVariant<uint> Castagnoli { get; } = new(0x1edc6f41, uint.MaxValue, uint.MaxValue, true, true);
    public static CRCVariant<uint> AIXM { get; } = new(0x814141ab, 0, 0, false, false);
    public static CRCVariant<uint> AUTOSAR { get; } = new(0xf4acfb13, uint.MaxValue, uint.MaxValue, true, true);
    public static CRCVariant<uint> BASE91 { get; } = new(0xa833982b, uint.MaxValue, uint.MaxValue, true, true);
    public static CRCVariant<uint> BZip2 { get; } = new(0x04c11db7, uint.MaxValue, uint.MaxValue, false, false);
    public static CRCVariant<uint> CDROM { get; } = new(0x8001801b, 0, 0, true, true);
    public static CRCVariant<uint> POSIX { get; } = new(0x04c11db7, 0, uint.MaxValue, false, false);
    public static CRCVariant<uint> ISO { get; } = new(0x04c11db7, uint.MaxValue, uint.MaxValue, true, true);
    public static CRCVariant<uint> JAM { get; } = new(0x04c11db7, uint.MaxValue, 0, true, true);
    public static CRCVariant<uint> MEF { get; } = new(0x741b8cd7, uint.MaxValue, 0, true, true);
    public static CRCVariant<uint> MPEG2 { get; } = new(0x04c11db7, uint.MaxValue, 0, false, false);
    public static CRCVariant<uint> XFER { get; } = new(0x000000af, 0, 0, false, false);
}
