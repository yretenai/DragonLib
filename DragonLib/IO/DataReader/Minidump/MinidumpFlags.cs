// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO.DataReader.Minidump;

[Flags]
public enum MinidumpFlags : ulong {
	Normal = 0x00000000,
	WithDataSegs = 0x00000001,
	WithFullMemory = 0x00000002,
	WithHandleData = 0x00000004,
	FilterMemory = 0x00000008,
	ScanMemory = 0x00000010,
	WithUnloadedModules = 0x00000020,
	WithIndirectlyReferencedMemory = 0x00000040,
	FilterModulePaths = 0x00000080,
	WithProcessThreadData = 0x00000100,
	WithPrivateReadWriteMemory = 0x00000200,
	WithoutOptionalData = 0x00000400,
	WithFullMemoryInfo = 0x00000800,
	WithThreadInfo = 0x00001000,
	WithCodeSegs = 0x00002000,
	WithoutAuxiliaryState = 0x00004000,
	WithFullAuxiliaryState = 0x00008000,
	WithPrivateWriteCopyMemory = 0x00010000,
	IgnoreInaccessibleMemory = 0x00020000,
	WithTokenInformation = 0x00040000,
	WithModuleHeaders = 0x00080000,
	FilterTriage = 0x00100000,
	WithAvxXStateContext = 0x00200000,
	WithIptTrace = 0x00400000,
	ScanInaccessiblePartialPages = 0x00800000,
	FilterWriteCombinedMemory,
	ValidTypeFlags = 0x01ffffff,
}
