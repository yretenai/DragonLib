// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;

namespace DragonLib.IO.Binary;

public class MemoryPoolBinaryReader : MemoryBinaryReader {
	public MemoryPoolBinaryReader(IMemoryOwner<byte> memory, bool leaveOpen = false) : base(memory.Memory) {
		MemoryPool = memory;
		LeaveOpen = leaveOpen;
	}

	public IMemoryOwner<byte> MemoryPool { get; }
	public bool LeaveOpen { get; }

	protected override void Dispose(bool disposing) {
		if (LeaveOpen) {
			return;
		}

		MemoryPool.Dispose();
	}
}
