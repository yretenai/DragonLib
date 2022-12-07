namespace DragonLib.Hash.Basis;

// https://reveng.sourceforge.io/crc-catalogue/all.htm
public static class CRC64Variants {
    public static CRCVariant<ulong> Default => ISO;
    public static CRCVariant<ulong> ISO { get; } = new(0x000000000000001b, ulong.MaxValue, ulong.MaxValue, true, true);
    public static CRCVariant<ulong> ECMA182 { get; } = new(0x42f0e1eba9ea3693, 0, 0, false, false);
    public static CRCVariant<ulong> Wolfgang { get; } = new(0x42f0e1eba9ea3693, ulong.MaxValue, ulong.MaxValue, false, false);
    public static CRCVariant<ulong> ECMA { get; } = new(0x42f0e1eba9ea3693, ulong.MaxValue, ulong.MaxValue, true, true);
    public static CRCVariant<ulong> Microsoft { get; } = new(0x259c84cba6426349, ulong.MaxValue, 0, true, true);
    public static CRCVariant<ulong> PyJones { get; } = new(0xad93d23594c935a9, ulong.MaxValue, 0, true, true); // PyCrc incorrectly lists crc-64-jones' init as 0xFFFF_FFFF_FFFF_FFFF, no real implementation has this.
    public static CRCVariant<ulong> Jones { get; } = new(0xad93d23594c935a9, 0, 0, true, true); // rust/redis/cern validate against this.
}
