// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.IO.DataReader;

public interface IMemoryHandler : IDisposable {
	static IMemoryHandler Handler { get; set; } = default!;
	bool ReadBytes(nint address, Span<byte> buffer, out int bytesRead);
	IEnumerable<(nint ModuleStart, int ModuleSize, string ModuleName)> EnumerateModules();
	Dictionary<string, List<(nint ModuleStart, int ModuleSize, SectionFlags Flags)>> MapSections();
}
