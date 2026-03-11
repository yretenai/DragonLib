// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO.Binary;

public class StrictStreamBinaryReader : StreamBinaryReader {
	public StrictStreamBinaryReader(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen) { }
	public StrictStreamBinaryReader(string path, FileAccess access = FileAccess.Read, FileShare share = FileShare.ReadWrite, bool leaveOpen = false) : this(new FileStream(path, FileMode.Open, access, share), leaveOpen) { }
	public StrictStreamBinaryReader(FileInfo fileInfo, FileAccess access = FileAccess.Read, FileShare share = FileShare.ReadWrite, bool leaveOpen = false) : this(new FileStream(fileInfo.FullName, FileMode.Open, access, share), leaveOpen) { }

	public override int Position { get => (int) BaseStream.Position; set => BaseStream.Position = value; }
}
