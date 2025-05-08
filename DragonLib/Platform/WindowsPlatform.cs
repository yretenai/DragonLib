// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using LUID = ulong;

namespace DragonLib.Platform;

internal static partial class WindowsPlatform {
	[Flags]
	public enum TokenAccessMask : uint {
		Delete = 0x00010000,
		ReadControl = 0x00020000,
		WriteDAC = 0x00040000,
		WriteOwner = 0x00080000,
		AccessSystemSecurity = 0x01000000,
		AssignPrimary = 0x00000001,
		Duplicate = 0x00000002,
		Impersonate = 0x00000004,
		Query = 0x00000008,
		QuerySource = 0x00000010,
		AdjustPrivileges = 0x00000020,
		AdjustGroups = 0x00000040,
		AdjustDefault = 0x00000080,
		AdjustSessionId = 0x00000100,
		Read = 0x00020008,
		Write = 0x000200E0,
		Execute = 0x00020000,
		TrustConstraintMask = 0x00020018,
		AccessPseudoHandle = 0x00000018,
		AllAccess = 0x000F01FF,
	}

	public enum TokenInformationClass {
		User = 1,
		Groups = 2,
		Privileges = 3,
		Owner = 4,
		PrimaryGroup = 5,
		DefaultDacl = 6,
		Source = 7,
		Type = 8,
		ImpersonationLevel = 9,
		Statistics = 10,
		RestrictedSids = 11,
		SessionId = 12,
		GroupsAndPrivileges = 13,
		SessionReference = 14,
		SandBoxInert = 15,
		AuditPolicy = 16,
		Origin = 17,
		ElevationType = 18,
		LinkedToken = 19,
		Elevation = 20,
		HasRestrictions = 21,
		AccessInformation = 22,
		VirtualizationAllowed = 23,
		VirtualizationEnabled = 24,
		IntegrityLevel = 25,
		UiAccess = 26,
		MandatoryPolicy = 27,
		LogonSid = 28,
		IsAppContainer = 29,
		Capabilities = 30,
		AppContainerSid = 31,
		AppContainerNumber = 32,
		UserClaimAttributes = 33,
		DeviceClaimAttributes = 34,
		RestrictedUserClaimAttributes = 35,
		RestrictedDeviceClaimAttributes = 36,
		DeviceGroups = 37,
		RestrictedDeviceGroups = 38,
		SecurityAttributes = 39,
		IsRestricted = 40,
		ProcessTrustLevel = 41,
		PrivateNameSpace = 42,
		SingletonAttributes = 43,
		BnoIsolation = 44,
		ChildProcessFlags = 45,
		IsLessPrivilegedAppContainer = 46,
		IsSandboxed = 47,
		IsAppSilo = 48,
		LoggingInformation = 49,
	}

	public static unsafe bool HasSECreateSymbolicLinkPrivilege() {
		if (!OperatingSystem.IsWindowsVersionAtLeast(6)) {
			return false;
		}

		using var identity = WindowsIdentity.GetCurrent();
		var principal = new WindowsPrincipal(identity);
		if (principal.IsInRole(WindowsBuiltInRole.Administrator)) {
			return true;
		}

		if (!NativeMethods.LookupPrivilegeValueW(default, "SeCreateSymbolicLinkPrivilege", out var symlinkLUID)) {
			return false;
		}

		if (!NativeMethods.OpenProcessToken(Process.GetCurrentProcess().Handle, TokenAccessMask.Query | TokenAccessMask.Read, out var tokenHandle)) {
			return false;
		}

		NativeMethods.GetTokenInformation(tokenHandle, TokenInformationClass.Privileges, nint.Zero, 0, out var size);
		if (size <= 0) {
			return false;
		}

		var buffer = stackalloc byte[size];
		if (!NativeMethods.GetTokenInformation(tokenHandle, TokenInformationClass.Privileges, (nint) buffer, size, out _)) {
			return false;
		}

		var tokenPrivileges = MemoryMarshal.Read<TokenPrivileges>(new ReadOnlySpan<byte>(buffer, size));
		var privileges = new ReadOnlySpan<LUIDAttributes>(tokenPrivileges.Privileges, tokenPrivileges.PrivilegeCount);
		foreach (var privilege in privileges) {
			if (privilege.LUID == symlinkLUID) {
				return true;
			}
		}

		return false;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct LUIDAttributes {
		public LUID LUID;
		public uint Attributes;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct TokenPrivileges {
		public int PrivilegeCount;
		public unsafe LUIDAttributes* Privileges;
	}

	private static partial class NativeMethods {
		[LibraryImport("advapi32", SetLastError = true)] [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static partial bool OpenProcessToken(nint handle, TokenAccessMask desiredAccess, out nint tokenHandle);

		[LibraryImport("advapi32", SetLastError = true)] [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static partial bool GetTokenInformation(nint tokenHandle, TokenInformationClass tokenInformationClass, nint tokenInformation, int tokenInformationLength, out int returnLength);

		[LibraryImport("advapi32", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)] [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static partial bool LookupPrivilegeValueW(string? systemName, string name, out LUID luid);
	}
}
