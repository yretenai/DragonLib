// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;

namespace DragonLib.IO.Binary;

public class ArrayBinaryReader : BufferBinaryReader {
	public ArrayBinaryReader(byte[] array, bool leaveOpen = false) {
		Array = array;
		LeaveOpen = leaveOpen;
	}

	public byte[] Array { get; }
	public bool LeaveOpen { get; }
	public override int Position { get; set; }

	public override void ReadBytes(Span<byte> span) => Array.AsSpan(Position, span.Length).CopyTo(span);

	protected override void Dispose(bool disposing) {
		if (LeaveOpen) {
			return;
		}

		ArrayPool<byte>.Shared.Return(Array);
	}
}
