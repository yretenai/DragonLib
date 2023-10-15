using DragonLib.IO;

namespace DragonLib.TestProgram;

[BitPacked(typeof(long))]
public record struct BitFieldTest {
    [BitField(4)]
    public int TestA { get; }

    [BitField(6)]
    public int TestB { get; }

    [BitField(12)]
    public int TestC { get; }
}
