namespace DragonLib.IO.DataReader.Minidump;

[Flags]
public enum MinidumpMemoryState : uint {
	Commit = 0x1000,
	Reserve = 0x2000,
	Free = 0x10000,
}
