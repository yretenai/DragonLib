using System;
using System.Collections.Generic;
using System.Text;

namespace DragonLib.Audio.WWise
{
    public class BNK
    {
        public BNK(Span<byte> buffer)
        {
            var cursor = 0;
            while (cursor < buffer.Length)
            {
                var magic = Encoding.ASCII.GetString(buffer.Slice(cursor, 4));
                BNKSection instance = magic switch
                {
                    "BKHD" => new BKHD(buffer[cursor..]),
                    "DATA" => new DATA(buffer[cursor..]),
                    "DIDX" => new DIDX(buffer[cursor..]),
                    // "ENVS" => new ENVS(buffer.Slice(cursor)),
                    // "FXPR" => new FXPR(buffer.Slice(cursor)),
                    "HIRC" => new HIRC(buffer[cursor..]),
                    // "STID" => new STID(buffer.Slice(cursor)),
                    _ => new DATA(buffer[cursor..]),
                };
                cursor += 8 + instance.Length;
                Sections.Add(instance);
            }
        }

        public List<BNKSection> Sections { get; } = new();
    }
}
