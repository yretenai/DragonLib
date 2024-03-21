using System.Buffers;

namespace DragonLib.Unsafe;

public class MemoryBufferStream : Stream {
	public unsafe MemoryBufferStream(Memory<byte> buffer) {
		Handle = buffer.Pin();
		Stream = new UnmanagedMemoryStream((byte*) Handle.Pointer, buffer.Length);
	}

	private UnmanagedMemoryStream Stream { get; }
	private MemoryHandle Handle { get; }

	public override bool CanRead => Stream.CanRead;

	public override bool CanSeek => Stream.CanSeek;

	public override bool CanWrite => Stream.CanWrite;

	public override long Length => Stream.Length;

	public override long Position {
		get => Stream.Position;
		set => Stream.Position = value;
	}

	public override void Flush() {
		Stream.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count) => Stream.Read(buffer, offset, count);

	public override long Seek(long offset, SeekOrigin origin) => Stream.Seek(offset, origin);

	public override void SetLength(long value) {
		Stream.SetLength(value);
	}

	public override void Write(byte[] buffer, int offset, int count) {
		Stream.Write(buffer, offset, count);
	}

	public override void Close() {
		base.Close();
		Stream.Close();
		Handle.Dispose();
	}

	protected override void Dispose(bool disposing) {
		base.Dispose(disposing);
		Stream.Dispose();
		Handle.Dispose();
	}
}
