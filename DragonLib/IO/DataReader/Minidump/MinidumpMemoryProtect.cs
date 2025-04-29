// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO.DataReader.Minidump;

[Flags]
public enum MinidumpMemoryProtect : uint {
	NoAccess = 0x00000001,
	ReadOnly = 0x00000002,
	ReadWrite = 0x00000004,
	Write = 0x00000008,
	Execute = 0x00000010,
	ExecuteRead = 0x00000020,
	ExecuteReadWrite = 0x00000040,
	ExecuteWrite = 0x00000080,
	Guard = 0x00000100,
	NoCache = 0x00000200,
	WriteCombine = 0x00000400,
	GraphicsNoAccess = 0x00000800,
	GraphicsRead = 0x00001000,
	GraphicsReadWrite = 0x00002000,
	GraphicsExecute = 0x00004000,
	GraphicsExecuteRead = 0x00008000,
	GraphicsExecuteReadWrite = 0x00010000,
	GraphicsCoherent = 0x00020000,
	GraphicsNoCache = 0x00040000,
}
