// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;

namespace DragonLib.IO.Binary;

public class MemoryPoolBinaryWriter : MemoryBinaryWriter {
	public MemoryPoolBinaryWriter(IMemoryOwner<byte> memory) : base(memory.Memory) => MemoryPool = memory;

	public MemoryPoolBinaryWriter() : this(MemoryPool<byte>.Shared.Rent(0xffff)) { }
	public MemoryPoolBinaryWriter(int capacity) : this(MemoryPool<byte>.Shared.Rent(capacity)) { }

	public IMemoryOwner<byte> MemoryPool { get; private set; }

	public override void EnsureCapacity(int length) {
		if (Capacity > length) {
			return;
		}

		var newMemory = MemoryPool<byte>.Shared.Rent(length.Align(0xffff));
		Memory.CopyTo(newMemory.Memory);
		MemoryPool.Dispose();
		MemoryPool = newMemory;
	}

	protected override void Dispose(bool disposing) => MemoryPool.Dispose();
}
