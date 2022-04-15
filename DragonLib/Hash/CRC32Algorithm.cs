using System.Runtime.Intrinsics.X86;
using DragonLib.Hash.Generic;

namespace DragonLib.Hash;

public class CRC32Algorithm : SpanHashAlgorithm<uint> {
    private readonly bool X64 = Sse42.X64.IsSupported;

    public CRC32Algorithm() {
        if (!Sse42.IsSupported) {
            throw new NotSupportedException("SSE4.2 instructions are not supported on this processor.");
        }

        HashSizeValue = 32;
    }

    public override int InputBlockSize => 32;
    public override int OutputBlockSize => 32;

    public new static CRC32Algorithm Create() => new();

    protected override void HashCore(byte[] array, int ibStart, int cbSize) {
        if (X64) {
            while (cbSize >= 8) {
                Value = (uint) Sse42.X64.Crc32(Value, BitConverter.ToUInt64(array, ibStart));
                ibStart += 8;
                cbSize -= 8;
            }
        }

        while (cbSize > 0) {
            Value = Sse42.Crc32(Value, array[ibStart++]);
            cbSize--;
        }
    }

    protected override byte[] HashFinal() => BitConverter.GetBytes(~Value);

    public override void Initialize() {
        Value = 0xFFFFFFFF;
    }
}
