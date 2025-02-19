using System.Runtime.InteropServices;

namespace DragonLib.IO.DataReader.Minidump;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public record struct MinidumpMemoryInfo {
	public nint BaseAddress { get; set; }
	public nint AllocationBase { get; set; }
	public MinidumpMemoryProtect AllocationProtect { get; set; }
	public ulong RegionSize { get; set; }
	public MinidumpMemoryState State { get; set; }
	public MinidumpMemoryProtect Protect { get; set; }
	public MinidumpMemoryType Type { get; set; }
}
