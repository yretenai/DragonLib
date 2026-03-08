// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DragonLib.IO.DataReader;

public sealed partial class IOVHandler : IMemoryHandler {
	public IOVHandler(int pid) {
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
			throw new InvalidOperationException("This handler only functions on Linux");
		}

		Process = Process.GetProcessById(pid);
	}

	public Process Process { get; }

	public unsafe bool ReadBytes(nint address, Span<byte> buffer, out int bytesRead) {
		fixed (byte* pin = buffer) {
			var localIo = new IOV { iov_base = (nint) pin, iov_len = (nuint) buffer.Length };

			var remoteIo = new IOV { iov_base = address, iov_len = (nuint) buffer.Length };

			nint read;
			if ((read = NativeMethods.process_vm_readv(Process.Id, ref localIo, 1, ref remoteIo, 1, 0)) == -1) {
				bytesRead = 0;
				return false;
			}

			bytesRead = (int) read;
		}

		return true;
	}

	public IEnumerable<(nint ModuleStart, int ModuleSize, string ModuleName)> EnumerateModules() {
		using var stream = new FileStream($"/proc/{Process.Id}/maps", FileMode.Open, FileAccess.Read);
		using var reader = new StreamReader(stream);

		var lastModule = string.Empty;
		while (reader.ReadLine() is { } line) {
			if (line.Length == 0) {
				continue;
			}

			var arr = line.Split(' ', 6, StringSplitOptions.RemoveEmptyEntries);
			var index = arr[0].IndexOf('-', StringComparison.Ordinal);
			var start = nint.Parse(arr[0][..index], NumberStyles.HexNumber);
			var end = nint.Parse(arr[0][(index + 1)..], NumberStyles.HexNumber);
			var moduleName = arr.ElementAtOrDefault(5);
			if (string.IsNullOrEmpty(moduleName)) {
				moduleName = lastModule;
			} else {
				lastModule = moduleName;
			}

			yield return (start, (int) (end - start), moduleName);
		}
	}

	public Dictionary<string, List<(nint ModuleStart, int ModuleSize, SectionFlags Flags)>> MapSections() {
		using var stream = new FileStream($"/proc/{Process.Id}/maps", FileMode.Open, FileAccess.Read);
		using var reader = new StreamReader(stream);

		var lastModule = string.Empty;
		var result = new Dictionary<string, List<(nint ModuleStart, int ModuleSize, SectionFlags Flags)>>();
		while (reader.ReadLine() is { } line) {
			if (line.Length == 0) {
				continue;
			}

			var arr = line.Split(' ', 6, StringSplitOptions.RemoveEmptyEntries);
			var index = arr[0].IndexOf('-', StringComparison.Ordinal);
			var start = nint.Parse(arr[0][..index], NumberStyles.HexNumber);
			var end = nint.Parse(arr[0][(index + 1)..], NumberStyles.HexNumber);
			var perm = arr[1];
			var moduleName = arr.ElementAtOrDefault(5);
			if (string.IsNullOrEmpty(moduleName)) {
				moduleName = lastModule;
			} else {
				lastModule = moduleName;
			}

			if (!result.TryGetValue(moduleName, out var sections)) {
				result[moduleName] = sections = [];
			}

			var flags = SectionFlags.NoAccess;

			if (perm.Length > 0 && perm[0] == 'r') {
				flags |= SectionFlags.Read;
			}

			if (perm.Length > 1 && perm[1] == 'w') {
				flags |= SectionFlags.Write;
			}

			if (perm.Length > 2 && perm[2] == 'x') {
				flags |= SectionFlags.Execute;
			}

			sections.Add((start, (int) (end - start), flags));
		}

		return result;
	}

	public void Dispose() {
		// nothing
	}

	[StructLayout(LayoutKind.Explicit, Size = 16)]
	private record struct IOV {
		[FieldOffset(0)]
		public nint iov_base;

		[FieldOffset(8)]
		public nuint iov_len;
	}

	private static partial class NativeMethods {
		[LibraryImport("libc")] [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
		internal static unsafe partial nint process_vm_readv(int pid, ref IOV localIov, nuint localIovCount, ref IOV remoteIov, nuint remoteIovCount, nuint flags);
	}
}
