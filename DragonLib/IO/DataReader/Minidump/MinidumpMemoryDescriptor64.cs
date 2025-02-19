using System.Runtime.InteropServices;

namespace DragonLib.IO.DataReader.Minidump;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public record struct MinidumpMemoryDescriptor64 {
	public nint StartOfMemoryRange { get; set; }
	public long Size { get; set; }
}
