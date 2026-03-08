// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;

namespace DragonLib.IO.Binary;

public class ArrayPoolBinaryWriter : ArrayBinaryWriter {
	public ArrayPoolBinaryWriter(byte[] array) : base(array) { }
	public ArrayPoolBinaryWriter() : this(ArrayPool<byte>.Shared.Rent(0xffff)) { }
	public ArrayPoolBinaryWriter(int capacity) : this(ArrayPool<byte>.Shared.Rent(capacity)) { }

	public override void EnsureCapacity(int length) {
		if (Capacity > length) {
			return;
		}

		var newArray = ArrayPool<byte>.Shared.Rent(length.Align(0xffff));
		Array.CopyTo(newArray);
		ArrayPool<byte>.Shared.Return(Array);
		Array = newArray;
	}

	protected override void Dispose(bool disposing) => ArrayPool<byte>.Shared.Return(Array);
}
