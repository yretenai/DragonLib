namespace DragonLib.IO.DataReader.Minidump;

[Flags]
public enum MinidumpMemoryType : uint {
	Private = 0x20000,
	Mapped = 0x40000,
	Image = 0x1000000,
}
