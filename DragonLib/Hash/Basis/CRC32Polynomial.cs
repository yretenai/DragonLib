namespace DragonLib.Hash.Basis;

// https://users.ece.cmu.edu/~koopman/crc/crc32.html
public enum CRC32Polynomial : uint {
    Default = IEEE,
    AIXM = 0xd5828281, // AIXM; CRC-32Q
    Castagnoli = 0x82f63b78, // iSCSI; CRC-32C; CRC-32/4
    Eight = 0xa814498f, // CRC-32/8
    IEEE = 0xedb88320, // IEEE 802.3
    Ray32sub8 = 0xb7800000, // Ray32sub8
    Six = 0xc8df356f, // CRC-32/6 original
    SixCorrected = 0xc8df352f, // CRC-32/6 corrected
    Variant = 0xedb88120, // CRC-32 variant

    Koopman2002 = 0x80108400, // Koopman 2002
    Koopman6sub8 = 0xa7000000, // CRC-32K/6sub8
    Koopman9 = 0xb5f4ff5c, // CRC-32K/9
    Koopman10 = 0xb49c1c96, // CRC-32K/10
    Koopman11 = 0xdc354ed0, // CRC-32K/11
    Koopman12 = 0xbafeb854, // CRC-32K/12
    Koopman13 = 0xec6ce6e4, // CRC-32K/13
    Koopman14 = 0xd6fa9482, // CRC-32K/14
    Koopman15 = 0xa34a7522, // CRC-32K/15
    Koopman16 = 0xedc3048b, // CRC-32K/16
    Koopman17 = 0xed93eb0a, // CRC-32K/17
    Koopman18 = 0xc56fae74, // CRC-32K/18
    Koopman3 = 0xf5000000, // FP-32; CRC-32K/3
    Koopman3_1 = 0xe792105a, // CRC-32K/3.1
    Koopman3_2 = 0xdff14cdc, // CRC-32K/3.2
    Koopman3_3 = 0xfbc76238, // CRC-32K/3.3
    Koopman3_4 = 0xf7069f80, // CRC-32K/3.4
    Koopman3_5 = 0xb271c170, // CRC-32K/3.5
    Koopman3_6 = 0xf07b81a5, // CRC-32K/3.6
    Koopman3_7 = 0xb5da8e66, // CRC-32K/3.7
    Koopman3_8 = 0xdd105d14, // CRC-32K/3.8
    Koopman3_9 = 0xf8b653b8, // CRC-32K/3.9
    Koopman3_10 = 0xf173f5c0, // CRC-32K/3.10
    Koopman3_11 = 0xb3434970, // CRC-32K/3.11
    Koopman3_12 = 0x95bb1718, // CRC-32K/3.12
    Koopman3_13 = 0xd9e95fb8, // CRC-32K/3.13
    Koopman3_14 = 0xf5196f13, // CRC-32K/3.14
    Koopman4_1 = 0xd8ea0000, // CRC-32K/4.1
    Koopman4_2 = 0xd79025c9, // CRC-32K/4.2
    Koopman5_1 = 0xd419cc15, // CRC-32/5.1
    Koopman5_2 = 0xa14eb4ea, // CRC-32/5.2
    Koopman6_1 = 0xeb31d82e, // CRC-32K/6.1
    Koopman6_2 = 0x992c1a4c, // CRC-32K/6.2
    Koopman6_3 = 0x90022004, // CRC-32K/6.3 
    Koopman6_4 = 0x9960034c, // CRC-32K/6.4
}
