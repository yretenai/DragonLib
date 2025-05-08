// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonLib.IO.Binary;

public abstract class BufferBinaryWriter : IDisposable {
	/// <summary>
	///     Gets or sets the current position in the data stream
	/// </summary>
	public abstract int Position { get; set; }

	public abstract int Length { get; protected set; }

	public abstract int Capacity { get; }


	/// <inheritdoc cref="IDisposable.Dispose" />
	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Ensure the underlying write buffer has enough space.
	/// </summary>
	/// <param name="length">Length to allocate to</param>
	public abstract void EnsureCapacity(int length);

	/// <summary>
	///     Write the specified amount of bytes to the buffer
	/// </summary>
	/// <param name="arr">Array to write from</param>
	public virtual void WriteBytes(RentedArray<byte> arr) => WriteBytes(arr.Span);

	/// <summary>
	///     Write span into the buffer
	/// </summary>
	/// <param name="span">The span to write from</param>
	public abstract void WriteBytes(ReadOnlySpan<byte> span);

	/// <summary>
	///     Write a single element of type <typeparamref name="T" />
	/// </summary>
	/// <typeparam name="T">Type to write</typeparam>
	/// <param name="value">Value to write</param>
	public virtual void Write<T>(T value) where T : struct => WriteBytes(MemoryMarshal.AsBytes(new Span<T>(ref value)));

	/// <summary>
	///     Writes an array of type <typeparamref name="T" />
	/// </summary>
	/// <param name="value">Number of elements to write</param>
	/// <typeparam name="T">Type to write</typeparam>
	public virtual void Write<T>(RentedArray<T> value) where T : struct => Write(value.Span);

	/// <summary>
	///     Writes an array of type <typeparamref name="TElement" /> with a <typeparamref name="TSize" /> size element
	/// </summary>
	/// <typeparam name="TSize">Type of the size specifier</typeparam>
	/// <typeparam name="TElement">Type to write</typeparam>
	public virtual void Write<TSize, TElement>(RentedArray<TElement> value) where TSize : struct, INumber<TSize> where TElement : struct {
		Write(TSize.CreateChecked(value.Length));
		if (value.Length == 0) {
			return;
		}

		Write(value);
	}

	/// <summary>
	///     Writes a span of type <typeparamref name="T" />
	/// </summary>
	/// <param name="span">Span to write from</param>
	/// <typeparam name="T">Type to write</typeparam>
	public virtual void Write<T>(ReadOnlySpan<T> span) where T : struct => WriteBytes(MemoryMarshal.AsBytes(span));

	/// <summary>
	///     Guess the encoding based on size
	/// </summary>
	/// <param name="encoding">User provided encoding</param>
	/// <param name="size">Size of a single element</param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException">Thrown when encoding type cannot be guessed</exception>
	protected virtual Encoding GuessEncoding(Encoding? encoding, int size) =>
		encoding switch {
			not null => encoding,
			null when size == 4 => Encoding.UTF32,
			null when size == 2 => Encoding.Unicode,
			null when size == 1 => Encoding.UTF8,
			_ => throw new InvalidOperationException(),
		};

	/// <summary>
	///     Writes a C-String (null-terminated string) with char type <typeparamref name="T" />
	/// </summary>
	/// <param name="text">Text to write</param>
	/// <param name="encoding">Encoding to write as</param>
	/// <param name="bufferSize">Size of the write buffer, must be at least as much as the string size</param>
	/// <param name="fixedSize">When false, rewind to the first byte after the null byte</param>
	/// <typeparam name="T">Type of a single char</typeparam>
	public virtual void WriteCString<T>(string text, Encoding? encoding = null, int bufferSize = 1024, bool fixedSize = false) where T : unmanaged, INumber<T> {
		// ReSharper disable once ArrangeRedundantParentheses
		var buffer = (stackalloc T[bufferSize]);
		buffer.Clear();

		encoding ??= GuessEncoding(encoding, Unsafe.SizeOf<T>());
		encoding.GetBytes(text, MemoryMarshal.AsBytes(buffer));
		var length = fixedSize ? -1 : buffer.IndexOf(T.Zero);
		if (length == -1) {
			buffer[^1] = T.Zero;
			length = buffer.Length;
		}

		Write(buffer[..length]);
	}

	/// <summary>
	///     Writes a Pascal-String (length-prefixed string) with char type <typeparamref name="TElement" />
	/// </summary>
	/// <param name="text">Text to write</param>
	/// <param name="encoding">Encoding to encode as</param>
	/// <typeparam name="TSize">Type of the size specifier</typeparam>
	/// <typeparam name="TElement">Type of a single char</typeparam>
	public virtual void WritePString<TSize, TElement>(string text, Encoding? encoding = null) where TSize : struct, INumber<TSize> where TElement : unmanaged, INumber<TSize> {
		encoding ??= GuessEncoding(encoding, Unsafe.SizeOf<TElement>());
		var length = encoding.GetByteCount(text);
		Write(TSize.CreateChecked(length));
		if (length == 0) {
			return;
		}

		// ReSharper disable once ArrangeRedundantParentheses
		var buffer = (stackalloc byte[length]);
		encoding.GetBytes(text, buffer);
		Write(buffer);
	}

	/// <summary>
	///     Skip number of <typeparamref name="T" /> elements
	/// </summary>
	/// <param name="count">Number of elements to skip</param>
	/// <typeparam name="T">Element type to skip</typeparam>
	public virtual void Skip<T>(int count = 1) where T : unmanaged => Position += Unsafe.SizeOf<T>() * count;

	/// <summary>
	///     Skip number of bytes
	/// </summary>
	/// <param name="count">bytes to skip</param>
	public virtual void Skip(int count) => Position += count;

	/// <summary>
	///     Align the buffer to the specified width
	/// </summary>
	/// <param name="n">Width to align to</param>
	public virtual void Align(int n = 4) => Position = Position.Align(n);

	protected abstract void Dispose(bool disposing);
}
