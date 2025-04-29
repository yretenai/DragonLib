// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace DragonLib.IO.DataReader.Minidump;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public record struct MinidumpModule {
	public nint BaseOfImage { get; set; }
	public int SizeOfImage { get; set; }
	public uint Checksum { get; set; }
	public uint Timestamp { get; set; }
	public uint ModuleNameRVA { get; set; }
	public MinidumpFileInfo VersionInfo { get; set; }
	public MinidumpLocationDescriptor CV { get; set; }
	public MinidumpLocationDescriptor Misc { get; set; }
	public ulong Reserved0 { get; set; }
	public ulong Reserved1 { get; set; }
}
