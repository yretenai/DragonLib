// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO.Binary;

public class ArrayBinaryWriter : BufferBinaryWriter {
	public ArrayBinaryWriter(byte[] array) => Array = array;

	public byte[] Array { get; protected set; }
	public override int Position { get; set; }
	public override int Length { get; protected set; }
	public override int Capacity => Array.Length;

	public override void EnsureCapacity(int length) {
		if (Capacity > length) {
			return;
		}

		var newArray = new byte[length.Align(0xffff)];
		Array.CopyTo(newArray);
		Array = newArray;
	}

	public override void WriteBytes(ReadOnlySpan<byte> span) {
		EnsureCapacity(Position + span.Length);

		span.CopyTo(Array.AsSpan(Position, span.Length));
		Position += span.Length;

		if (Position > Length) {
			Length = Position;
		}
	}

	protected override void Dispose(bool disposing) { }
}
