// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO.Binary;

public class ArrayBinaryReader : BufferBinaryReader {
	public ArrayBinaryReader(byte[] array) => Array = array;

	public byte[] Array { get; }
	public override int Position { get; set; }
	public override int Length => Array.Length;

	public override void ReadBytes(Span<byte> span) {
		Array.AsSpan(Position, span.Length).CopyTo(span);
		Position += span.Length;
	}

	protected override void Dispose(bool disposing) { }
}
