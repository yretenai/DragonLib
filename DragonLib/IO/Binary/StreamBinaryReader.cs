// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO.Binary;

public class StreamBinaryReader : BufferBinaryReader {
	public StreamBinaryReader(Stream stream, bool leaveOpen = false) {
		BaseStream = stream;
		LeaveOpen = leaveOpen;
	}

	public StreamBinaryReader(string path, FileAccess access = FileAccess.Read, FileShare share = FileShare.ReadWrite, bool leaveOpen = false) : this(new FileStream(path, FileMode.Open, access, share), leaveOpen) { }

	public StreamBinaryReader(FileInfo fileInfo, FileAccess access = FileAccess.Read, FileShare share = FileShare.ReadWrite, bool leaveOpen = false) : this(new FileStream(fileInfo.FullName, FileMode.Open, access, share), leaveOpen) { }

	public Stream BaseStream { get; }
	public bool LeaveOpen { get; }
	public override int Position { get => (int) BaseStream.Position; set => BaseStream.Position = value; }
	public override int Length => (int) BaseStream.Length;

	public override void ReadBytes(Span<byte> span) => BaseStream.ReadExactly(span);

	protected override void Dispose(bool disposing) {
		if (LeaveOpen) {
			return;
		}

		BaseStream.Close();
		BaseStream.Dispose();
	}
}
