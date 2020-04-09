using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace DragonLib.Audio.WWise
{
    // Data Index
    [PublicAPI]
    public class DIDX : BNKSection
    {
        public DIDX(Span<byte> data) : base(data) => Entries = MemoryMarshal.Cast<byte, DIDXEntry>(data.Slice(8)).ToArray();

        public DIDXEntry[] Entries { get; }
    }

    [PublicAPI]
    public struct DIDXEntry
    {
        public uint Id { get; }
        public int Offset { get; }
        public int Length { get; }
    }
}
