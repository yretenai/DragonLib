namespace DragonLib.Hash.Basis;

// https://users.ece.cmu.edu/~koopman/crc/crc64.html
public enum CRC64Polynomial : ulong {
    Default = ISO,
    ECMA = 0xc96c5795d7870f42, // CRC-64-ECMA 182
    FOP = 0xea00000000000000, // FOP-64
    ISO = 0xd800000000000000, // FP-64, CRC-64-ISO 3309
    Jones = 0x95ac9329ac4bc9b5, // Jones
    SixSubEight = 0x8f00000000000000, // CRC-64/6sub8x
}
