using System.Diagnostics;
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

	public void Dispose() {
		// nothing
	}

	private static partial class NativeMethods {
		[LibraryImport("kernel32", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static partial bool ReadProcessMemory(SafeProcessHandle processHandle, nint address, nint bytes, nint size, ref nint bytesReadCount);
	}
}
