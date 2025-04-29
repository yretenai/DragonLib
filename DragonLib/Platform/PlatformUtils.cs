// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.Platform;

public static class PlatformUtils {
	public static bool CanCreateSymlinks => !OperatingSystem.IsWindows() || WindowsPlatform.HasSECreateSymbolicLinkPrivilege();
}
