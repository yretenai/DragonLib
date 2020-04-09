using System;
using System.Buffers.Binary;
using System.Text;
using JetBrains.Annotations;

namespace DragonLib.Audio.WWise
{
    [PublicAPI]
    public class BNKSection
    {
        public BNKSection(Span<byte> data)
        {
            Magic = Encoding.ASCII.GetString(data.Slice(0, 4));
            Length = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(4));
        }

        public string Magic { get; }
        public int Length { get; }
    }
}
