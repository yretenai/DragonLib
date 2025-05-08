// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;

namespace DragonLib.IO.Binary;

public sealed class RentedArray<T> : IDisposable {
	public RentedArray(T[] array, int length) {
		Array = array;
		Length = length;
	}

	public RentedArray(int length) : this(ArrayPool<T>.Shared.Rent(length), length) { }

	public RentedArray() {
		Length = 0;
		Array = [];
	}

	public T[] Array { get; private set; }
	public int Length { get; private set; }
	public Span<T> Span => Length == 0 ? Span<T>.Empty : Array.AsSpan(0, Length);

	public void Dispose() {
		if (Length == 0) {
			return;
		}

		ArrayPool<T>.Shared.Return(Array);
		Array = [];
		Length = 0;
	}
}
