using System;

namespace DragonLib.Asset.Sections {
    public class MemorySection : BaseSection {
        public MemorySection(DragonAssetSectionHeader header, Memory<byte> buffer) : base(header, buffer) {
            Buffer = buffer;
        }

        public Memory<byte> Buffer { get; set; }

        public override Memory<byte> WriteSection() {
            return Buffer;
        }
    }
}
