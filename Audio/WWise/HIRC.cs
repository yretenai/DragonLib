using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using DragonLib.Audio.WWise.Hierarchy;
using JetBrains.Annotations;

namespace DragonLib.Audio.WWise
{
    // Hierarchy
    [PublicAPI]
    public class HIRC : BNKSection
    {
        public HIRC(Span<byte> data) : base(data)
        {
            Count = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(8));
            var cursor = 12;
            for (var i = 0; i < Count; ++i)
            {
                var kind = (HIRCSectionEnum) data[cursor];
                var section = kind switch
                {
                    //HIRCSectionEnum.Settings => ,
                    //HIRCSectionEnum.SoundEffect => ,
                    //HIRCSectionEnum.EventAction => ,
                    //HIRCSectionEnum.Event => ,
                    //HIRCSectionEnum.RandomSequenceContainer => ,
                    //HIRCSectionEnum.SwitchContainer => ,
                    //HIRCSectionEnum.ActorMixer => ,
                    //HIRCSectionEnum.AudioBus => ,
                    //HIRCSectionEnum.BlendContainer => ,
                    //HIRCSectionEnum.MusicSegment => ,
                    //HIRCSectionEnum.MusicTrack => ,
                    //HIRCSectionEnum.MusicSwitch => ,
                    //HIRCSectionEnum.MusicPlaylist => ,
                    //HIRCSectionEnum.Attenuation => ,
                    //HIRCSectionEnum.DialogueEvent => ,
                    //HIRCSectionEnum.MotionBus => ,
                    //HIRCSectionEnum.MotionEffect => ,
                    //HIRCSectionEnum.Effect => ,
                    //HIRCSectionEnum.AuxiliaryBus => ,
                    _ => new HIRCSection(data.Slice(cursor))
                };
                Sections.Add(section);
                cursor += 5 + section.Length;
            }
        }

        public int Count { get; }

        public List<HIRCSection> Sections { get; } = new List<HIRCSection>();
    }
}
