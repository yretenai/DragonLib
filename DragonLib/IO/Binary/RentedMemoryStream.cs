namespace DragonLib.IO.Binary;

public class RentedMemoryStream : Stream {
	public RentedMemoryStream(int capacity, bool writable = true) {
		Buffer = new RentedArray<byte>(capacity);
		CanWrite = writable;
	}

	public RentedMemoryStream(byte[] buffer, int length = 0, bool writable = true) {
		Buffer = new RentedArray<byte>(buffer, length <= 0 ? buffer.Length : length);
		CanWrite = writable;
	}

	public RentedMemoryStream(Span<byte> buffer, bool writable = true) {
		Buffer = new RentedArray<byte>(buffer.Length);
		buffer.CopyTo(Buffer.Span);
		CanWrite = writable;
	}

	public RentedMemoryStream(RentedArray<byte> buffer, bool leaveOpen = false, bool writable = true) {
		Buffer = buffer;
		LeaveOpen = leaveOpen;
		CanWrite = writable;
	}

	public RentedArray<byte> Buffer { get; }
	public override void Flush() { }

	public override void Close() {
		if (IsOpen && !LeaveOpen) {
			Buffer.Dispose();
		}

		IsOpen = false;
	}

	public bool IsOpen { get; private set; }
	private bool LeaveOpen { get; } = true;

	private void EnsureNotClosed() {
		if (IsOpen) {
			return;
		}

		throw new ObjectDisposedException(nameof(Buffer));
	}

	private void EnsureWriteable() {
		if (!CanWrite) {
			throw new NotSupportedException("Unwritable stream");
		}
	}

	public override int Read(byte[] buffer, int offset, int count) {
		ValidateBufferArguments(buffer, offset, count);
		EnsureNotClosed();

		var n = Math.Min((int) (Length - Position), count);
		if (n <= 0) {
			return 0;
		}

		Buffer.Array.AsSpan((int) Position, n).CopyTo(buffer.AsSpan(offset));
		Position += n;
		return n;
	}

	public override int Read(Span<byte> buffer) {
		EnsureNotClosed();
		var n = Math.Min((int) (Length - Position), buffer.Length);
		if (n <= 0) {
			return 0;
		}

		Buffer.Array.AsSpan((int) Position, n).CopyTo(buffer);
		Position += n;
		return n;
	}

	public override long Seek(long offset, SeekOrigin origin) {
		EnsureNotClosed();

		switch (origin) {
			case SeekOrigin.Begin: {
				break;
			}
			case SeekOrigin.Current: {
				offset += Position;
				break;
			}
			case SeekOrigin.End: {
				offset = Length + offset;
				break;
			}
			default: throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
		}

		ArgumentOutOfRangeException.ThrowIfNegative(offset);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, Length);

		Position = offset;
		return Position;
	}

	public override void SetLength(long value) {
		if (value != Length) {
			throw new NotSupportedException();
		}
	}

	public override void Write(byte[] buffer, int offset, int count) {
		ValidateBufferArguments(buffer, offset, count);
		EnsureNotClosed();
		EnsureWriteable();

		var n = Math.Min((int) (Length - Position), count);
		if (n <= 0) {
			return;
		}

		buffer.AsSpan(offset, count).CopyTo(Buffer.Array.AsSpan((int) Position, n));
		Position += n;
	}

	public override void Write(ReadOnlySpan<byte> buffer) {
		EnsureNotClosed();
		EnsureWriteable();

		var n = Math.Min((int) (Length - Position), buffer.Length);
		if (n <= 0) {
			return;
		}

		buffer.CopyTo(Buffer.Array.AsSpan((int) Position, n));
		Position += n;
	}

	public override int ReadByte() {
		EnsureNotClosed();
		return Buffer.Array[Position++];
	}

	public override void WriteByte(byte value) {
		EnsureNotClosed();
		EnsureWriteable();
		Buffer.Array[Position++] = value;
	}

	public override void CopyTo(Stream destination, int bufferSize) {
		EnsureNotClosed();
		destination.Write(Buffer.Array.AsSpan((int) Position));
	}

	public override bool CanRead => true;
	public override bool CanSeek => true;
	public override bool CanWrite { get; }
	public override long Length => Buffer.Length;

	public override long Position {
		get {
			EnsureNotClosed();
			return field;
		}
		set {
			EnsureNotClosed();
			ArgumentOutOfRangeException.ThrowIfGreaterThan(value, Length);
			field = value;
		}
	}
}
