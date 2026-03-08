// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO.DataReader.Minidump;

[Flags]
public enum MinidumpMemoryState : uint {
	Commit = 0x1000,
	Reserve = 0x2000,
	Free = 0x10000,
}
