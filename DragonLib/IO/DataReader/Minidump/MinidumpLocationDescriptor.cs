// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace DragonLib.IO.DataReader.Minidump;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public record struct MinidumpLocationDescriptor {
	public int Size { get; set; }
	public int RVA { get; set; }
}
