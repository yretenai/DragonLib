using System;
using JetBrains.Annotations;

namespace DragonLib.Asset.Sections
{
    [PublicAPI]
    public class MemorySection : BaseSection
    {
        public Memory<byte> Buffer { get; set; }
        
        public MemorySection(DragonAssetSectionHeader header, Memory<byte> buffer) : base(header, buffer)
        {
            Buffer = buffer;
        }

        public override Memory<byte> WriteSection() => Buffer;
    }
}
