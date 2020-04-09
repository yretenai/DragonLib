using JetBrains.Annotations;

namespace DragonLib.Audio.WAV
{
    [PublicAPI]
    public struct WAVEData
    {
        public int Magic { get; set; }
        public int Size { get; set; }
    }
}
