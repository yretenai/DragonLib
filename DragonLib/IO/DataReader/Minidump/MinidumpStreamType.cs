namespace DragonLib.IO.DataReader.Minidump;

[Flags]
public enum MinidumpStreamType : uint {
	Unused = 0,
	ThreadList = 3,
	ModuleList = 4,
	MemoryList = 5,
	Exception = 6,
	SystemInfo = 7,
	ThreadExList = 8,
	Memory64List = 9,
	CommentA = 10,
	CommentW = 11,
	HandleData = 12,
	FunctionTable = 13,
	UnloadedModuleList = 14,
	MiscInfo = 15,
	MemoryInfoList = 16,
	ThreadInfoList = 17,
	HandleOperationList = 18,
	Token = 19,
	JavaScriptData = 20,
	SystemMemoryInfo = 21,
	ProcessVmCounters = 22,
	IptTrace = 23,
	ThreadNames = 24,
}
