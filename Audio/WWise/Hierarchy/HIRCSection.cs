using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace DragonLib.Audio.WWise.Hierarchy
{
    public abstract class HIRCSection
    {
        protected HIRCSection(Span<byte> data)
        {
            Type = MemoryMarshal.Read<HIRCSectionEnum>(data);
            Length = BinaryPrimitives.ReadInt32LittleEndian(data[1..]);
            Id = BinaryPrimitives.ReadUInt32LittleEndian(data[5..]);
        }

        public HIRCSectionEnum Type { get; }
        public int Length { get; }
        public uint Id { get; }
    }

    public class BlankHIRC : HIRCSection
    {
        public BlankHIRC(Span<byte> data) : base(data) => Buffer = data.Slice(9, Length - 4).ToArray();

        public byte[] Buffer { get; }
    }

    public enum HIRCSectionEnum : byte
    {
        Settings = 1,
        SoundEffect = 2,
        EventAction = 3,
        Event = 4,
        RandomSequenceContainer = 5,
        SwitchContainer = 6,
        ActorMixer = 7,
        AudioBus = 8,
        BlendContainer = 9,
        MusicSegment = 10,
        MusicTrack = 11,
        MusicSwitch = 12,
        MusicPlaylist = 13,
        Attenuation = 14,
        DialogueEvent = 15,
        MotionBus = 16,
        MotionEffect = 17,
        Effect = 18,
        AuxiliaryBus = 19,
    }
}
