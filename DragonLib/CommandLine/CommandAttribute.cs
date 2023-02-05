namespace DragonLib.CommandLine;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class CommandAttribute : Attribute {
    public CommandAttribute(Type flagsType, string name, string description, string group = "", bool hide = false) {
        FlagsType = flagsType;
        Name = name;
        Description = description;
        Group = group;
        Hide = hide;
    }

    public Type FlagsType { get; }
    public string Name { get; }
    public string Description { get; }
    public string Group { get; }
    public bool Hide { get; }
}
