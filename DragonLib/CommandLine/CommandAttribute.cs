namespace DragonLib.CommandLine;

public interface ICommandAttribute {
	public Type FlagsType { get; }
	public string Name { get; }
	public string Description { get; }
	public string Group { get; }
	public bool Hide { get; }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class CommandAttribute<T>(string Name, string Description, string Group = "", bool Hide = false) : Attribute, ICommandAttribute where T : CommandLineFlags {
	public string Name { get; } = Name;
	public string Description { get; } = Description;
	public string Group { get; } = Group;
	public bool Hide { get; } = Hide;
	public Type FlagsType { get; } = typeof(T);
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class CommandAttribute(Type FlagsType, string Name, string Description, string Group = "", bool Hide = false) : Attribute, ICommandAttribute {
	public string Name { get; } = Name;
	public string Description { get; } = Description;
	public string Group { get; } = Group;
	public bool Hide { get; } = Hide;
	public Type FlagsType { get; } = FlagsType;
}
