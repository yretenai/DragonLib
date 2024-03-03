namespace DragonLib.Bashcomp;

public record Option(string Help, List<string> Flags);
public record EnumValue(string Name, string Help);
public record EnumOption(string Name, string Help, List<string> Flags, List<EnumValue> Values) : Option(Help, Flags);
