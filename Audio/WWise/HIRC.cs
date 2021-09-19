using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using DragonLib.Audio.WWise.Hierarchy;

namespace DragonLib.Audio.WWise
{
    // Hierarchy
    public class HIRC : BNKSection
    {
        public HIRC(Span<byte> data) : base(data)
        {
            Count = BinaryPrimitives.ReadInt32LittleEndian(data[8..]);
            var cursor = 12;
            for (var i = 0; i < Count; ++i)
            {
                var kind = (HIRCSectionEnum) data[cursor];
                HIRCSection section = kind switch
                {
                    HIRCSectionEnum.Settings => new Settings(data[cursor..]),
                    //HIRCSectionEnum.SoundEffect => ,
                    //HIRCSectionEnum.EventAction => ,
                    //HIRCSectionEnum.Event => ,
                    //HIRCSectionEnum.RandomSequenceContainer => ,
                    //HIRCSectionEnum.SwitchContainer => ,
                    //HIRCSectionEnum.ActorMixer => ,
                    //HIRCSectionEnum.AudioBus => ,
                    //HIRCSectionEnum.BlendContainer => ,
                    //HIRCSectionEnum.MusicSegment => ,
                    HIRCSectionEnum.MusicTrack => new MusicTrack(data[cursor..]),
                    //HIRCSectionEnum.MusicSwitch => ,
                    //HIRCSectionEnum.MusicPlaylist => ,
                    //HIRCSectionEnum.Attenuation => ,
                    //HIRCSectionEnum.DialogueEvent => ,
                    //HIRCSectionEnum.MotionBus => ,
                    //HIRCSectionEnum.MotionEffect => ,
                    //HIRCSectionEnum.Effect => ,
                    //HIRCSectionEnum.AuxiliaryBus => ,
                    _ => new BlankHIRC(data[cursor..]),
                };
                cursor += 5 + section.Length;
                Sections.Add(section);
            }
        }

        public int Count { get; }

        public List<HIRCSection> Sections { get; } = new();
    }
}
