namespace DragonLib.Platform;

public static class PlatformUtils {
	public static bool CanCreateSymlinks => !OperatingSystem.IsWindows() || WindowsPlatform.HasSECreateSymbolicLinkPrivilege();
}
