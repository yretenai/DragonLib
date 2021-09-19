using System;
using System.Collections.Generic;

namespace DragonLib.Audio.WWise.Hierarchy {
    public class Settings : HIRCSection {
        public Settings(Span<byte> data) : base(data) {
            var count = data[9];
            for (var i = 0; i < count; ++i) Values[(SettingType)data[10 + i]] = BitConverter.ToSingle(data[(10 + count + i * 4)..]);
        }

        public Dictionary<SettingType, float> Values { get; } = new();
    }

    public enum SettingType : byte {
        Volume = 0,
        LowPassFilter = 3
    }
}
