using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using DragonLib.Asset.Sections;
using DragonLib.IO;
using JetBrains.Annotations;

namespace DragonLib.Asset
{
    [PublicAPI]
    public class DragonAsset
    {
        public DragonSection Header { get; set; }
        public List<BaseSection> Sections { get; set; }
        
        public static Dictionary<DragonAssetSectionId, Type> SectionTypes = BaseSection.Type.Assembly.GetTypes().Where(x => BaseSection.Type.IsAssignableFrom(x)).Select(x => (Id: x.GetCustomAttribute<DragonIdAttribute>()?.Id ?? 0, Type: x)).Where(x => x.Id != 0).ToDictionary(x => x.Id, x => x.Type);

        public DragonAsset(Memory<byte> buffer)
        {
            var ptr = 0;
            var section = ReadSection(buffer, ref ptr);
            if (section.Header.Magic != DragonAssetSectionId.Dragon)
            {
                if (section.Header.Magic == DragonAssetSectionId.BigDragon)
                {
                    throw new InvalidOperationException("Big Endian files are unsupported");
                }
                throw new InvalidDataException("Not a Dragon asset file");
            }

            Header = (DragonSection) section;
            Sections = new List<BaseSection>(Header.Header.Count);
            for (var i = 0; i < Sections.Count; ++i)
            {
                Sections.Add(ReadSection(buffer, ref ptr));
            }
        }

        private BaseSection ReadSection(Memory<byte> buffer, ref int ptr)
        {
            var header = MemoryMarshal.Read<DragonAssetSectionHeader>(buffer.Span.Slice(ptr));
            var sz = DragonAssetSectionHeader.SectionHeaderSize;
            if(header.Size < sz) throw new InvalidDataException($"Section size is under minimum of {sz} bytes");
            var chunk = header.Size == DragonAssetSectionHeader.SectionHeaderSize ? Memory<byte>.Empty : buffer.Slice(ptr + sz, header.Size - sz);
            ptr += header.Size;
            if (SectionTypes.TryGetValue(header.Magic, out var type))
            {
                return (BaseSection) Activator.CreateInstance(type, header, chunk);
            }
            Logger.Warn("DRAGON", $"Unhandled section type {header.Magic:G}");
            return new MemorySection(header, chunk);
        }

        public Span<byte> Write()
        {
            throw new NotImplementedException();
        }
    }
}
