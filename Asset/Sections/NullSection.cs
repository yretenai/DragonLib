﻿using System;

namespace DragonLib.Asset.Sections
{
    [DragonId(DragonAssetSectionId.Null)]
    public class NullSection : BaseSection
    {
        public NullSection() : base(DragonAssetSectionId.Null, Guid.NewGuid()) { }

        public NullSection(DragonAssetSectionHeader header, Memory<byte> buffer) : base(header, buffer) { }

        public override Memory<byte> WriteSection() => Memory<byte>.Empty;
    }
}
