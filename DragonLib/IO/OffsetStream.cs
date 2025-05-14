// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO;

public class OffsetStream : Stream {
	public OffsetStream(Stream stream, long? offset = null, long? length = null, bool leaveOpen = false) {
		BaseStream = stream;
		Start = offset ?? stream.Position;
		End = Start + (length ?? stream.Length - Start);
		BaseStream.Position = Start;
		LeaveOpen = leaveOpen;
	}

	private Stream BaseStream { get; }
	public long Start { get; }
	public long End { get; }
	public bool LeaveOpen { get; }
	public bool IsOpen { get; private set; } = true;

	public override bool CanRead => BaseStream.CanRead;

	public override bool CanSeek => BaseStream.CanSeek;

	public override bool CanWrite => BaseStream.CanWrite;

	public override long Length => End - Start;

	public override long Position {
		get => BaseStream.Position - Start;
		set => Seek(value, SeekOrigin.Begin);
	}

	public override void Close() {
		IsOpen = false;

		if (LeaveOpen) {
			return;
		}

		BaseStream.Close();
	}

	private void EnsureNotClosed() {
		if (IsOpen) {
			return;
		}

		throw new ObjectDisposedException(nameof(Buffer));
	}

	private void EnsureWriteable() {
		if (!CanWrite) {
			throw new IOException("Unwritable stream");
		}
	}

	public override void Flush() {
		EnsureNotClosed();
		BaseStream.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count) {
		EnsureNotClosed();

		if (Position < 0 || Position > Length) {
			throw new IOException("Underlying stream is incorrectly reused");
		}

		if (BaseStream.Position + count > End) {
			count = (int) (End - BaseStream.Position);
		}

		return count <= 0 ? 0 : BaseStream.Read(buffer, offset, count);
	}

	public override long Seek(long offset, SeekOrigin origin) {
		EnsureNotClosed();

		var absolutePosition = origin switch {
			SeekOrigin.Begin => Start + offset,
			SeekOrigin.Current => BaseStream.Position + offset,
			SeekOrigin.End => End + offset,
			_ => throw new ArgumentOutOfRangeException(nameof(origin), origin, default),
		};

		if (absolutePosition > End) {
			throw new IOException("Attempting to seek past the end of the stream");
		}

		if (absolutePosition < Start) {
			throw new IOException("Attempting to seek to a negative value");
		}

		BaseStream.Seek(absolutePosition, SeekOrigin.Begin);
		return Position;
	}

	public override void SetLength(long value) => throw new NotSupportedException();

	public override void Write(byte[] buffer, int offset, int count) {
		EnsureNotClosed();
		EnsureWriteable();

		if (Position < 0 || Position > Length) {
			throw new IOException("Underlying stream is incorrectly reused");
		}

		if (BaseStream.Position + count > End) {
			count = (int) (End - BaseStream.Position);
		}

		if (count <= 0) {
			return;
		}

		BaseStream.Write(buffer, offset, count);
	}
}
