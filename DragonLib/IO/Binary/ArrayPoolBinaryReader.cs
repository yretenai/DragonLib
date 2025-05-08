// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;

namespace DragonLib.IO.Binary;

public class ArrayPoolBinaryReader : ArrayBinaryReader {
	public ArrayPoolBinaryReader(byte[] array, bool leaveOpen = false) : base(array) => LeaveOpen = leaveOpen;

	public bool LeaveOpen { get; }

	protected override void Dispose(bool disposing) {
		if (LeaveOpen) {
			return;
		}

		ArrayPool<byte>.Shared.Return(Array);
	}
}
