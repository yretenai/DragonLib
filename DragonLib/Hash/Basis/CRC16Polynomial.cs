namespace DragonLib.Hash.Basis;

// https://users.ece.cmu.edu/~koopman/crc/crc16.html
public enum CRC16Polynomial : ushort {
    Default = IBM,
    ARINC = 0xd405, // CRC_16-ARINC
    C2 = 0xf94f, // C2
    CCITT = 0x8408, // CCITT-16
    CMDA = 0xe613, // CRC-16-CMDA2000
    Chakravarty = 0xa8f4, // Chakravarty
    DECT = 0x91a0, // CRC-16-DECT
    DNP = 0xa6bc, // C1; CRC-16-DNP; CRC-16F/6.2
    FOP = 0xe000, // FOP-16
    FP16 = 0xb400, // FP-16
    IBM = 0xc002, // CRC-16; CRC-16-IBM
    IBM16 = 0xe905, // IBM-16
    IEC = 0xc9da, // IEC TC57
    IEEE = 0xc6f6, // IEEE WG77.1
    ProfiBus = 0xf3b8, // ProfiBus
    Q = 0xde91, // CRC-16Q
    SCSI = 0xedd1, // CRC-16-T10-DIF
    SixSubEight = 0xd880, // C4; CRC-16/6sub8

    Koopman3 = 0xeaf1, // CRC-16K/3
    Koopman4 = 0xdaae, // CRC-16K/4 
    Koopman5 = 0xf234, // CRC-16K/5
    Koopman6 = 0xa7b0, // CRC-16K/6
    Koopman7_1 = 0xe8b4, // CRC-16K/7.1
    Koopman7_2 = 0xda0e, // CRC-16K/7.2
    Koopman9 = 0xfa4b, // CRC-16K/9
    Koopman10 = 0xfa5b, // CRC-16K/10

    Fast3 = 0xd4d8, // CRC-16F/3
    Fast4_1 = 0xb842, // CRC-16F/4.1 
    Fast4_2 = 0xd745, // C5; CRC-16F/4.2
    Fast5 = 0xac9a, // C3; CRC-16F/5 
    Fast6_1 = 0xeae9, // CRC-16F/6.1 
    Fast7 = 0xad64, // CRC-16F/7
    Fast8 = 0xcf88, // CRC-16F/8
    Fast8_2 = 0xedf8, // CRC-16F/8_2
    Fast9 = 0xfb1a, // CRC-16F/9
    Fast10_1 = 0xbcf4, // CRC-16F/10.1
    Fast10_2 = 0xf174, // CRC-16F/10.2
    Fast11 = 0xfedf, // CRC-16F/11
    Fast12 = 0xdf56, // CRC-16F/12
    Fast14 = 0xfffb, // CRC-16F/14
}
