namespace DragonLib.Hash.Basis;

// https://users.ece.cmu.edu/~koopman/crc/crc8.html
public enum CRC8Polynomial : byte {
    Default = DVB,
    AUTOSAR = 0xf4, // CRC-8-AUTOSAR; CRC-8F/4.2 
    Bluetooth = 0xe5, // CRC-8-Bluetooth
    CCITT = 0xc6, // CCITT-8
    DARC = 0x9c, // DARC-8
    DVB = 0xab, // DVB; CRC-8
    Dallas = 0x8c, // Dallas/Maxim, DOWCRC
    FOP, // FOP-8; ATM-8; CRC-8P
    GSM = 0x92, // CRC-8-GSM-B
    SAE = 0xb8, // SAE J-1850; FP-8
    WCDMA = 0xd9, // WCDMA-8

    Koopman3 = 0xb2, // CRC-8K/3

    Fast3 = 0xf3, // CRC-8F/3
    Fast4_1 = 0xd8, // CRC-8F/4.1
    Fast5 = 0xeb, // CRC-8F/5 
    Fast6 = 0xec, // CRC-8F/6 
    Fast8 = 0xfe, // CRC-8F/8
}
