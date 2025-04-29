// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO.DataReader.Minidump;

[Flags]
public enum MinidumpMemoryType : uint {
	Private = 0x20000,
	Mapped = 0x40000,
	Image = 0x1000000,
}
