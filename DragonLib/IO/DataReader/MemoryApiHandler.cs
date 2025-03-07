using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DragonLib.IO.DataReader;

public sealed partial class MemoryApiHandler : IMemoryHandler {
	public MemoryApiHandler(int pid) {
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
			throw new InvalidOperationException("This handler only functions on Windows");
		}

		Process = Process.GetProcessById(pid);
	}

	public Process Process { get; }


	public unsafe bool ReadBytes(nint address, Span<byte> buffer, out int bytesRead) {
		fixed (byte* pin = buffer) {
			var read = nint.Zero;
			if (!NativeMethods.ReadProcessMemory(Process.SafeHandle, address, (nint) pin, buffer.Length, ref read)) {
				bytesRead = 0;
				return false;
			}

			bytesRead = (int) read;
		}

		return true;
	}

	public IEnumerable<(nint ModuleStart, int ModuleSize, string ModuleName)> EnumerateModules() {
		for (var index = 0; index < Process.Modules.Count; index++) {
			var module = Process.Modules[index];
			yield return (module.BaseAddress, module.ModuleMemorySize, module.FileName);
		}
	}

	public Dictionary<string, List<(nint ModuleStart, int ModuleSize, SectionFlags Flags)>> MapSections() {
		var result = new Dictionary<string, List<(nint ModuleStart, int ModuleSize, SectionFlags Flags)>>();
		foreach (var (moduleStart, moduleSize, moduleName) in EnumerateModules()) {
			var sections = new List<(nint ModuleStart, int ModuleSize, SectionFlags Flags)>();
			result[moduleName] = sections;
			var currentAddress = moduleStart;
			while (currentAddress < moduleSize) {
				if (NativeMethods.VirtualQueryEx(Process.SafeHandle, currentAddress, out var region, Unsafe.SizeOf<MemoryBasicInformation64>()) == 0) {
					if (Marshal.GetLastPInvokeError() == 0x57) {
						break;
					}

					throw new Win32Exception();
				}

				var flags = SectionFlags.NoAccess;
				if ((region.Protect & ProtectionType.Execute) != 0) {
					flags |= SectionFlags.Execute;
				}

				if ((region.Protect & ProtectionType.ExecuteRead) != 0) {
					flags |= SectionFlags.Execute;
					flags |= SectionFlags.Read;
				}

				if ((region.Protect & ProtectionType.ExecuteRead) != 0) {
					flags |= SectionFlags.Execute;
					flags |= SectionFlags.Read;
				}

				if ((region.Protect & ProtectionType.ExecuteReadWrite) != 0) {
					flags |= SectionFlags.Execute;
					flags |= SectionFlags.Read;
					flags |= SectionFlags.Write;
				}

				if ((region.Protect & ProtectionType.ExecuteWriteCopy) != 0) {
					flags |= SectionFlags.Execute;
					flags |= SectionFlags.Write;
				}

				if ((region.Protect & ProtectionType.WriteCopy) != 0) {
					flags |= SectionFlags.Write;
				}

				if ((region.Protect & ProtectionType.ReadOnly) != 0) {
					flags |= SectionFlags.Read;
				}

				if ((region.Protect & ProtectionType.ReadWrite) != 0) {
					flags |= SectionFlags.Read;
					flags |= SectionFlags.Write;
				}

				sections.Add((region.BaseAddress, (int) region.RegionSize, flags));
				currentAddress = region.BaseAddress + region.RegionSize;
			}
		}

		return result;
	}

	public void Dispose() {
		// nothing
	}


	[Flags, SuppressMessage("ReSharper", "UnusedMember.Local")]
	private enum AllocationType {
		Commit = 0x1000,
		Reserve = 0x2000,
		DeCommit = 0x4000,
		Release = 0x8000,
		Free = 0x10000,
		Private = 0x20000,
		Mapped = 0x40000,
		Reset = 0x80000,
		TopDown = 0x100000,
	}

	[Flags, SuppressMessage("ReSharper", "UnusedMember.Local")]
	private enum ProtectionType {
		NoAccess = 0x01,
		ReadOnly = 0x02,
		ReadWrite = 0x04,
		WriteCopy = 0x08,
		Execute = 0x10,
		ExecuteRead = 0x20,
		ExecuteReadWrite = 0x40,
		ExecuteWriteCopy = 0x80,
		Guard = 0x100,
		NoCache = 0x200,
	}

	[StructLayout(LayoutKind.Explicit, Size = 48)]
	// ReSharper disable once NotAccessedPositionalProperty.Local
	private readonly record struct MemoryBasicInformation64([field: FieldOffset(0x0)] nint BaseAddress, [field: FieldOffset(0x18)] nint RegionSize, [field: FieldOffset(0x20)] AllocationType State, [field: FieldOffset(0x24)] ProtectionType Protect);

	private static partial class NativeMethods {
		[LibraryImport("kernel32", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static partial bool ReadProcessMemory(SafeProcessHandle processHandle, nint address, nint bytes, nint size, ref nint bytesReadCount);

		[LibraryImport("kernel32", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		internal static partial nint VirtualQueryEx(SafeProcessHandle processHandle, nint address, out MemoryBasicInformation64 memoryInformation, nint size);
	}
}
