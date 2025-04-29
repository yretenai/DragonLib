// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;

namespace DragonLib.IO.Binary;

public sealed class RentedArray<T> : IDisposable {
	public RentedArray(T[] array, int size) {
		Array = array;
		Size = size;
	}

	public RentedArray(int length) : this(ArrayPool<T>.Shared.Rent(length), length) { }

	public RentedArray() {
		Size = 0;
		Array = [];
	}

	public T[] Array { get; private set; }
	public int Size { get; private set; }
	public Span<T> Span => Size == 0 ? Span<T>.Empty : Array.AsSpan(0, Size);

	public void Dispose() {
		if (Size == 0) {
			return;
		}

		ArrayPool<T>.Shared.Return(Array);
		Array = [];
		Size = 0;
	}
}
