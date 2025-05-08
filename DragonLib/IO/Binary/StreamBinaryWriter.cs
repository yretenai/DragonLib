// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO.Binary;

public class StreamBinaryWriter : BufferBinaryWriter {
	public StreamBinaryWriter(Stream stream, bool leaveOpen = false) {
		BaseStream = stream;
		LeaveOpen = leaveOpen;
	}

	public Stream BaseStream { get; }
	public bool LeaveOpen { get; }
	public override int Position { get => (int) BaseStream.Position; set => BaseStream.Position = value; }

	public override int Length {
		get => (int) BaseStream.Length;
		protected set { }
	}

	public override int Capacity => (int) BaseStream.Length;

	public override void EnsureCapacity(int length) {
		if (length > Capacity) {
			BaseStream.SetLength(length);
		}
	}

	public override void WriteBytes(ReadOnlySpan<byte> span) => BaseStream.Write(span);

	protected override void Dispose(bool disposing) {
		if (LeaveOpen) {
			return;
		}

		BaseStream.Close();
		BaseStream.Dispose();
	}
}
