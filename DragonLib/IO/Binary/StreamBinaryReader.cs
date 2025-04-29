// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO.Binary;

public class StreamBinaryReader : BufferBinaryReader {
	public StreamBinaryReader(Stream stream, bool leaveOpen = false) {
		BaseStream = stream;
		LeaveOpen = leaveOpen;
	}

	public Stream BaseStream { get; }
	public bool LeaveOpen { get; }
	public override int Position { get => (int) BaseStream.Position; set => BaseStream.Position = value; }

	public override void ReadBytes(Span<byte> span) => BaseStream.ReadExactly(span);

	protected override void Dispose(bool disposing) {
		if (LeaveOpen) {
			return;
		}

		BaseStream.Close();
		BaseStream.Dispose();
	}
}
