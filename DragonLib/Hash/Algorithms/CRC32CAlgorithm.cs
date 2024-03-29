using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace DragonLib.Hash.Algorithms;

public sealed class CRC32CAlgorithm : SpanHashAlgorithm<uint> {
	private readonly bool X64 = Sse42.X64.IsSupported;

	public CRC32CAlgorithm() {
		if (!IsSupported) {
			throw new NotSupportedException("SSE4.2 instructions are not supported on this processor.");
		}

		HashSizeValue = 32;
		Reset();
	}

	public static bool IsSupported => Sse42.IsSupported;

	public override int InputBlockSize => 32;
	public override int OutputBlockSize => 32;

	public new static CRC32CAlgorithm Create() => new();

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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

	protected override uint GetValueFinal() {
		var val = ~Value;
		Reset();
		return val;
	}

	public override void Reset() => Value = uint.MaxValue;
	public override void Initialize() => Reset();
}
