namespace DragonLib.IO.DataReader;

[Flags]
public enum SectionFlags {
	NoAccess = 0,
	Read = 1,
	Write = 2,
	Execute = 4,
}
