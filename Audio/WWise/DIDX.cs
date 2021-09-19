using System;
using System.Runtime.InteropServices;

namespace DragonLib.Audio.WWise {
    // Data Index
    public class DIDX : BNKSection {
        public DIDX(Span<byte> data) : base(data) {
            Entries = MemoryMarshal.Cast<byte, DIDXEntry>(data[8..]).ToArray();
        }

        public DIDXEntry[] Entries { get; }
    }

    public struct DIDXEntry {
        public uint Id { get; }
        public int Offset { get; }
        public int Length { get; }
    }
}
