// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO.DataReader.Minidump;

[Flags]
public enum MinidumpMemoryState : uint {
	Commit = 0x1000,
	Reserve = 0x2000,
	Free = 0x10000,
}
