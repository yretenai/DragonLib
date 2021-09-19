using System;

namespace DragonLib.Audio.WWise {
    // Data
    public class DATA : BNKSection {
        public DATA(Span<byte> data) : base(data) {
            Buffer = new Memory<byte>(data.Slice(8, Length).ToArray());
        }

        public Memory<byte> Buffer { get; }
    }
}
