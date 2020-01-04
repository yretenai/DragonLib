using System;
using JetBrains.Annotations;

namespace DragonLib.Asset.Sections
{
    [PublicAPI]
    public class BaseSection
    {
        internal static Type Type = typeof(BaseSection);

        public BaseSection(DragonAssetSectionId id, Guid guid)
        {
            Header = new DragonAssetSectionHeader
            {
                Magic = id,
                Size = DragonAssetSectionHeader.SectionHeaderSize,
                Count = 0,
                Guid = guid
            };
        }

        public BaseSection(DragonAssetSectionHeader header, Memory<byte> buffer)
        {
            Header = header;
        }

        public DragonAssetSectionHeader Header { get; set; }

        public virtual Memory<byte> WriteSection()
        {
            throw new NotImplementedException();
        }
    }
}
