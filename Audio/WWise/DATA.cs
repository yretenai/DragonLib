using System;
using JetBrains.Annotations;

namespace DragonLib.Audio.WWise
{
    // Data
    [PublicAPI]
    public class DATA : BNKSection
    {
        public DATA(Span<byte> data) : base(data) => Buffer = new Memory<byte>(data.Slice(8).ToArray());

        public Memory<byte> Buffer { get; }
    }
}
