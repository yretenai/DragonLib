using System;
using JetBrains.Annotations;

namespace DragonLib.Asset.Sections
{
    [PublicAPI, DragonId(DragonAssetSectionId.Null)]
    public class NullSection : BaseSection
    {
        public NullSection() : base(DragonAssetSectionId.Null, Guid.NewGuid()) { }

        public NullSection(DragonAssetSectionHeader header, Memory<byte> buffer) : base(header, buffer) { }
        
        public override Memory<byte> WriteSection()
        {
            return Memory<byte>.Empty;
        }
    }
}
