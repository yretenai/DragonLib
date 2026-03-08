// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace DragonLib.IO.DataReader.Minidump;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public record struct MinidumpFileInfo {
	public uint Signature { get; set; }
	public uint StructVersion { get; set; }
	public uint FileVersionMS { get; set; }
	public uint FileVersionLS { get; set; }
	public uint ProductVersionMS { get; set; }
	public uint ProductVersionLS { get; set; }
	public uint FileFlagsMask { get; set; }
	public uint FileFlags { get; set; }
	public uint FileOS { get; set; }
	public uint FileType { get; set; }
	public uint FileSubtype { get; set; }
	public uint FileDateMS { get; set; }
	public uint FileDateLS { get; set; }
}
