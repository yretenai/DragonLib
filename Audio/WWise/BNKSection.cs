using System;
using System.Buffers.Binary;
using System.Text;

namespace DragonLib.Audio.WWise
{
    public abstract class BNKSection
    {
        protected BNKSection(Span<byte> data)
        {
            Magic = Encoding.ASCII.GetString(data[..4]);
            Length = BinaryPrimitives.ReadInt32LittleEndian(data[4..]);
        }

        public string Magic { get; }
        public int Length { get; }
    }
}
