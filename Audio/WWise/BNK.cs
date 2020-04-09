using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace DragonLib.Audio.WWise
{
    [PublicAPI]
    public class BNK
    {
        public BNK(Span<byte> buffer)
        {
            var cursor = 0;
            while (cursor < buffer.Length)
            {
                var magic = Encoding.ASCII.GetString(buffer.Slice(cursor, 4));
                var instance = magic switch
                {
                    "BKHD" => new BKHD(buffer.Slice(cursor)),
                    "DATA" => new DATA(buffer.Slice(cursor)),
                    "DIDX" => new DIDX(buffer.Slice(cursor)),
                    // "ENVS" => new ENVS(buffer.Slice(cursor)),
                    // "FXPR" => new FXPR(buffer.Slice(cursor)),
                    "HIRC" => new HIRC(buffer.Slice(cursor)),
                    // "STID" => new STID(buffer.Slice(cursor)),
                    _ => new BNKSection(buffer.Slice(cursor))
                };
                cursor += 8 + instance.Length;
                Sections.Add(instance);
            }
        }

        public List<BNKSection> Sections { get; } = new List<BNKSection>();
    }
}
