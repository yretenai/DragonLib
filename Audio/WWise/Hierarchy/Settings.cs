using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.Audio.WWise.Hierarchy
{
    [PublicAPI]
    public class Settings : HIRCSection
    {
        public Settings(Span<byte> data) : base(data)
        {
            var count = data[9];
            for (var i = 0; i < count; ++i) Values[(SettingType) data[10 + i]] = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(10 + count + i * 4));
        }

        public Dictionary<SettingType, float> Values { get; } = new Dictionary<SettingType, float>();
    }

    [PublicAPI]
    public enum SettingType : byte
    {
        Volume = 0,
        LowPassFilter = 3
    }
}
