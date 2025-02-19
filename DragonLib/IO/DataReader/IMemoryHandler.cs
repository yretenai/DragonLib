namespace DragonLib.IO.DataReader;

public interface IMemoryHandler : IDisposable {
	public static IMemoryHandler Handler { get; set; } = null!;
	public bool ReadBytes(nint address, Span<byte> buffer, out int bytesRead);
	public IEnumerable<(nint ModuleStart, int ModuleSize, string ModuleName)> EnumerateModules();
}
