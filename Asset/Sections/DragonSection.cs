using System;
using System.Buffers.Binary;
using System.IO;
using JetBrains.Annotations;

namespace DragonLib.Asset.Sections
{
    [PublicAPI]
    [DragonId(DragonAssetSectionId.Dragon)]
    public class DragonSection : BaseSection
    {
        public static readonly Guid StandardGuid = new Guid(new byte[] { 0xAB, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A, 0xAB, 0xAB, 0xBB, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A, 0xAB, 0xBB });

        public DragonSection() : base(DragonAssetSectionId.Dragon, StandardGuid) { }

        public DragonSection(DragonAssetSectionHeader header, Memory<byte> buffer) : base(header, buffer)
        {
            if (header.Guid != StandardGuid) throw new InvalidDataException("Dragon Asset GUID is non standard!");
            TotalSize = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Span);
            ZUp = buffer.Span[4] != 0;
        }

        public uint TotalSize { get; set; }
        public bool ZUp { get; set; }

        public override Memory<byte> WriteSection()
        {
            var buffer = new Memory<byte>(new byte[SizeHelper.SizeOf<uint>() + 1]);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Span, TotalSize);
            buffer.Span[4] = (byte) (ZUp ? 0 : 1);
            return buffer;
        }
    }
}
