// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonLib.IO.Binary;

public abstract class BufferBinaryReader : IDisposable {
	/// <summary>
	///     Gets or sets the current position in the data stream
	/// </summary>
	public abstract int Position { get; set; }

	/// <summary>
	///     Length of the data stream
	/// </summary>
	public abstract int Length { get; }

	/// <inheritdoc cref="IDisposable.Dispose" />
	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///     Read the specified amount of bytes into an array
	/// </summary>
	/// <param name="length">Number of bytes to read</param>
	/// <returns></returns>
	public virtual RentedArray<byte> ReadBytes(int length) {
		var arr = new RentedArray<byte>(length);
		ReadBytes(arr.Span);
		return arr;
	}

	/// <summary>
	///     Read the specified amount of bytes into an array without advancing the stream
	/// </summary>
	/// <param name="length">Number of bytes to read</param>
	/// <returns></returns>
	public virtual RentedArray<byte> PeekBytes(int length) {
		var pos = Position;
		var elems = ReadBytes(length);
		Position = pos;
		return elems;
	}

	/// <summary>
	///     Read the specified amount of bytes into a span
	/// </summary>
	/// <param name="span">The span to read into</param>
	public abstract void ReadBytes(Span<byte> span);

	/// <summary>
	///     Read the specified amount of bytes into a span without advancing the stream
	/// </summary>
	/// <param name="span">The span to read into</param>
	/// <returns></returns>
	public virtual void PeekBytes(Span<byte> span) {
		var pos = Position;
		ReadBytes(span);
		Position = pos;
	}

	/// <summary>
	///     Read a single element of type <typeparamref name="T" />
	/// </summary>
	/// <typeparam name="T">Type to read</typeparam>
	/// <returns></returns>
	public virtual T Read<T>() where T : struct {
		T elem = default;
		ReadBytes(MemoryMarshal.AsBytes(new Span<T>(ref elem)));
		return elem;
	}

	/// <summary>
	///     Read a single element of type <typeparamref name="T" /> without advancing the buffer
	/// </summary>
	/// <typeparam name="T">Type to read</typeparam>
	/// <returns></returns>
	public virtual T Peek<T>() where T : struct {
		var pos = Position;
		var elem = Read<T>();
		Position = pos;
		return elem;
	}

	/// <summary>
	///     Reads an array of type <typeparamref name="T" /> with a specified count
	/// </summary>
	/// <param name="length">Number of elements to read</param>
	/// <typeparam name="T">Type to read</typeparam>
	/// <returns></returns>
	public virtual RentedArray<T> Read<T>(int length) where T : struct {
		var arr = new RentedArray<T>(length);
		Read(arr.Span);
		return arr;
	}

	/// <summary>
	///     Reads an array of type <typeparamref name="T" /> with a specified count without advancing the buffer
	/// </summary>
	/// <param name="length">Number of elements to read</param>
	/// <typeparam name="T">Type to read</typeparam>
	/// <returns></returns>
	public virtual RentedArray<T> Peek<T>(int length) where T : struct {
		var pos = Position;
		var elems = Read<T>(length);
		Position = pos;
		return elems;
	}

	/// <summary>
	///     Reads an array of type <typeparamref name="TElement" /> with a <typeparamref name="TSize" /> size element
	/// </summary>
	/// <typeparam name="TSize">Type of the size specifier</typeparam>
	/// <typeparam name="TElement">Type to read</typeparam>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException">Thrown when size is negative</exception>
	public virtual RentedArray<TElement> Read<TSize, TElement>() where TSize : struct, INumber<TSize> where TElement : struct {
		var length = Read<TSize>();
		if (length == TSize.Zero) {
			return new RentedArray<TElement>();
		}

		if (length < TSize.Zero) {
			throw new InvalidOperationException();
		}

		var intLength = int.CreateChecked(length);
		return Read<TElement>(intLength);
	}

	/// <summary>
	///     Reads an array of type <typeparamref name="TElement" /> with a <typeparamref name="TSize" /> size element without advancing the buffer
	/// </summary>
	/// <typeparam name="TSize">Type of the size specifier</typeparam>
	/// <typeparam name="TElement">Type to read</typeparam>
	/// <returns></returns>
	public virtual RentedArray<TElement> Peek<TSize, TElement>() where TSize : struct, INumber<TSize> where TElement : struct {
		var pos = Position;
		var elems = Read<TSize, TElement>();
		Position = pos;
		return elems;
	}

	/// <summary>
	///     Reads a span of type <typeparamref name="T" />
	/// </summary>
	/// <param name="span">Span to read into</param>
	/// <typeparam name="T">Type to read</typeparam>
	/// <returns></returns>
	public virtual void Read<T>(Span<T> span) where T : struct => ReadBytes(MemoryMarshal.AsBytes(span));

	/// <summary>
	///     Reads a span of type <typeparamref name="T" /> without advancing the buffer
	/// </summary>
	/// <param name="span">Span to read into</param>
	/// <typeparam name="T">Type to read</typeparam>
	/// <returns></returns>
	public virtual void Peek<T>(Span<T> span) where T : struct {
		var pos = Position;
		Read(span);
		Position = pos;
	}

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
			_ when size == 4 => Encoding.UTF32,
			_ when size == 2 => Encoding.Unicode,
			_ when size == 1 => Encoding.UTF8,
			_ => throw new InvalidOperationException(),
		};

	/// <summary>
	///     Reads a C-String (nul-terminated string) with char type <typeparamref name="T" />
	/// </summary>
	/// <param name="encoding">Encoding to decode as</param>
	/// <param name="bufferSize">Size of the read buffer, must be at least as much as the string size</param>
	/// <param name="fixedSize">When false, rewind to the first byte after the null byte</param>
	/// <typeparam name="T">Type of a single char</typeparam>
	/// <returns></returns>
	public virtual string ReadCString<T>(Encoding? encoding = default, int bufferSize = 1024, bool fixedSize = false) where T : unmanaged, INumber<T> {
		// ReSharper disable once ArrangeRedundantParentheses
		var buffer = (stackalloc T[bufferSize]);
		var start = Position;
		Read(buffer);
		var length = buffer.IndexOf(T.Zero);
		var one = Unsafe.SizeOf<T>();
		try {
			switch (length) {
				case 0: return string.Empty;
				case < 0: length = buffer.Length; break;
			}

			return GuessEncoding(encoding, one).GetString(MemoryMarshal.AsBytes(buffer[..length]));
		} finally {
			if (!fixedSize) {
				Position = start + length + one;
			}
		}
	}

	/// <summary>
	///     Reads a C-String (null-terminated string) with char type <typeparamref name="T" /> without advancing the buffer
	/// </summary>
	/// <param name="encoding">Encoding to decode as</param>
	/// <param name="bufferSize">Size of the read buffer, must be at least as much as the string size</param>
	/// <param name="fixedSize">When false, rewind to the first byte after the null byte</param>
	/// <typeparam name="T">Type of a single char</typeparam>
	/// <returns></returns>
	public virtual string PeekCString<T>(Encoding? encoding = default, int bufferSize = 1024, bool fixedSize = false) where T : unmanaged, INumber<T> {
		var pos = Position;
		var value = ReadCString<T>(encoding, bufferSize, fixedSize);
		Position = pos;
		return value;
	}

	/// <summary>
	///     Reads a Pascal-String (length-prefixed string) with char type <typeparamref name="TElement" />
	/// </summary>
	/// <param name="encoding">Encoding to decode as</param>
	/// <param name="trim">Number of elements to remove from the end of the string</param>
	/// <typeparam name="TSize">Type of the size specifier</typeparam>
	/// <typeparam name="TElement">Type of a single char</typeparam>
	/// <returns></returns>
	public virtual string ReadPString<TSize, TElement>(Encoding? encoding = default, int trim = 0) where TSize : struct, INumber<TSize> where TElement : unmanaged, INumber<TSize> {
		var length = Read<TSize>();
		if (length == TSize.Zero) {
			return string.Empty;
		}

		var intLength = int.CreateChecked(length);
		if (intLength - trim <= 0) {
			Skip<TElement>(intLength);
			return string.Empty;
		}

		// ReSharper disable once ArrangeRedundantParentheses
		var buffer = (stackalloc TElement[intLength]);
		Read(buffer);
		return GuessEncoding(encoding, Unsafe.SizeOf<TElement>()).GetString(MemoryMarshal.AsBytes(buffer[..(intLength - trim)]));
	}

	/// <summary>
	///     Reads a Pascal-String (length-prefixed string) with char type <typeparamref name="TElement" /> without advancing the buffer
	/// </summary>
	/// <param name="encoding">Encoding to decode as</param>
	/// <param name="trim">Number of elements to remove from the end of the string</param>
	/// <typeparam name="TSize">Type of the size specifier</typeparam>
	/// <typeparam name="TElement">Type of a single char</typeparam>
	/// <returns></returns>
	public virtual string PeekPString<TSize, TElement>(Encoding? encoding = default, int trim = 0) where TSize : struct, INumber<TSize> where TElement : unmanaged, INumber<TSize> {
		var pos = Position;
		var value = ReadPString<TSize, TElement>(encoding, trim);
		Position = pos;
		return value;
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
