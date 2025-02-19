using System.Runtime.InteropServices;

namespace DragonLib.IO.DataReader.Minidump;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public record struct MinidumpHeader {
	public uint Signature { get; set; }
	public uint Version { get; set; }
	public int NumberOfStreams { get; set; }
	public uint StreamDirectoryRVA { get; set; }
	public uint Checksum { get; set; }
	public uint Timestamp { get; set; }
	public MinidumpFlags Flags { get; set; }
}
