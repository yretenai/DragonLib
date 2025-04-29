// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace DragonLib.IO.DataReader.Minidump;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct MinidumpMemoryInfo {
	public nint BaseAddress { get; set; }
	public nint AllocationBase { get; set; }
	public MinidumpMemoryProtect AllocationProtect { get; set; }
	public nint RegionSize { get; set; }
	public MinidumpMemoryState State { get; set; }
	public MinidumpMemoryProtect Protect { get; set; }
	public MinidumpMemoryType Type { get; set; }
}
