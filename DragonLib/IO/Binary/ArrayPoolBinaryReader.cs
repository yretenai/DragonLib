// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;

namespace DragonLib.IO.Binary;

public class ArrayPoolBinaryReader : ArrayBinaryReader {
	public ArrayPoolBinaryReader(byte[] array, int length, bool leaveOpen = false) : base(array) {
		LeaveOpen = leaveOpen;
		Length = length;
	}

	public ArrayPoolBinaryReader(RentedArray<byte> array, bool leaveOpen = false) : base(array.Array) {
		LeaveOpen = leaveOpen;
		Length = array.Length;
		Rented = array;
	}

	public override int Length { get; }
	protected RentedArray<byte>? Rented { get; }

	public bool LeaveOpen { get; }

	public override IRentedArray<T> ReadShared<T>(int length) where T : struct {
		if (Rented == null) {
			return new RentedArray<T>(0);
		}

		return new UnownedCovariantArray<T>(Rented, Position, length);
	}

	protected override void Dispose(bool disposing) {
		if (LeaveOpen) {
			return;
		}

		if (Rented != null) {
			Rented.Dispose();
			return;
		}

		ArrayPool<byte>.Shared.Return(Array);
	}
}
