namespace DragonLib.Audio.WAV
{
    public struct WAVEHeader
    {
        public int Magic { get; set; }
        public int Size { get; set; }
        public int Format { get; set; }
    }
}
