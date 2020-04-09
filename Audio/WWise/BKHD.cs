using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace DragonLib.Audio.WWise
{
    // Bank Header
    [PublicAPI]
    public class BKHD : BNKSection
    {
        public BKHD(Span<byte> data) : base(data) => Data = MemoryMarshal.Read<BKHDStruct>(data.Slice(8));

        public BKHDStruct Data { get; }
    }

    [PublicAPI]
    public struct BKHDStruct
    {
        public uint Version { get; }
        public uint Id { get; }
        public uint BankId { get; }
        public uint Reserved1 { get; }
        public int Unknown { get; }
        public uint Reserved2 { get; }
    }
}
