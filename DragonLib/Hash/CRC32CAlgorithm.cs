using System.Buffers.Binary;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;

namespace DragonLib.Hash;

public sealed class CRC32CAlgorithm : HashAlgorithm {
    private readonly bool X64 = Sse42.X64.IsSupported;

    public CRC32CAlgorithm() {
        if (!IsSupported) {
            throw new NotSupportedException("SSE4.2 instructions are not supported on this processor.");
        }

        HashSizeValue = 32;
        Reset();
    }

    public uint Value { get; private set; }

    public static bool IsSupported => Sse42.IsSupported;

    public override int InputBlockSize => 32;
    public override int OutputBlockSize => 32;

    public new static CRC32CAlgorithm Create() => new();

    protected override void HashCore(byte[] array, int ibStart, int cbSize) {
        if (X64) {
            var span = array.AsSpan();
            while (cbSize >= 8) {
                Value = (uint) Sse42.X64.Crc32(Value, BinaryPrimitives.ReadUInt64LittleEndian(span[ibStart..]));
                ibStart += 8;
                cbSize -= 8;
            }
        }

        while (cbSize > 0) {
            Value = Sse42.Crc32(Value, array[ibStart++]);
            cbSize--;
        }
    }

    public uint ComputeHashValue(Span<byte> bytes) {
        HashCore(bytes.ToArray(), 0, bytes.Length);
        return ~Value;
    }

    protected override byte[] HashFinal() => BitConverter.GetBytes(~Value);

    public void Reset() => Value = uint.MaxValue;
    public override void Initialize() => Reset();
}
