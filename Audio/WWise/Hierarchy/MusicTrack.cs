using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace DragonLib.Audio.WWise.Hierarchy
{
    [PublicAPI]
    public class MusicTrack : HIRCSection
    {
        public MusicTrack(Span<byte> data) : base(data)
        {
            var cursor = 9;
            Unknown = data[cursor++];
            var count = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(cursor));
            cursor += 4;
            var sz = SizeHelper.SizeOf<MusicTrackEntry>() * count;
            Tracks = MemoryMarshal.Cast<byte, MusicTrackEntry>(data.Slice(cursor, sz)).ToArray();
            cursor += sz;
            Leftover = data.Slice(cursor, Length - cursor + 5).ToArray(); // has shit like where it's from and duration.
        }

        public MusicTrackEntry[] Tracks { get; }
        public byte Unknown { get; }
        public byte[] Leftover { get; }
    }

    [PublicAPI]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MusicTrackEntry
    {
        public ushort Unknown1 { get; set; }
        public ushort Unknown2 { get; set; }
        public byte Unknown3 { get; set; }
        public uint StreamId { get; set; }
        public byte Weight { get; set; }
        public uint Unknown5 { get; set; }
    }
}
