using System;
using System.Runtime.InteropServices;

namespace DragonLib.Asset
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DragonAssetSectionHeader
    {
        public DragonAssetSectionId Magic { get; set; }
        public uint Version { get; set; }
        public Guid Guid { get; set; }
        public int Size { get; set; }
        public int Count { get; set; }

        internal static readonly int SectionHeaderSize = SizeHelper.SizeOf<DragonAssetSectionHeader>();
    }
}
