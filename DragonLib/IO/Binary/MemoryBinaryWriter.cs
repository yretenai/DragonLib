// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using DragonLib.Extensions;

namespace DragonLib.IO.Binary;

public class MemoryBinaryWriter : BufferBinaryWriter {
	public MemoryBinaryWriter(Memory<byte> memory) => Memory = memory;

	public Memory<byte> Memory { get; protected set; }
	public override int Position { get; set; }
	public override int Length { get; protected set; }
	public override int Capacity => Memory.Length;

	public override void EnsureCapacity(int length) {
		if (Capacity > length) {
			return;
		}

		var newArray = new byte[length.Align(0xffff)];
		Memory.CopyTo(newArray);
		Memory = newArray;
	}

	public override void WriteBytes(ReadOnlySpan<byte> span) {
		EnsureCapacity(Position + span.Length);

		span.CopyTo(Memory.Span.Slice(Position, span.Length));
		Position += span.Length;

		if (Position > Length) {
			Length = Position;
		}
	}

	protected override void Dispose(bool disposing) { }
}
