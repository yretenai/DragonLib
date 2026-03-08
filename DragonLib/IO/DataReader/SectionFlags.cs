// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO.DataReader;

[Flags]
public enum SectionFlags {
	NoAccess = 0,
	Read = 1,
	Write = 2,
	Execute = 4,
}
