// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace DragonLib.CommandLine;

public interface ICommandAttribute {
	Type FlagsType { get; }
	string Name { get; }
	string Description { get; }
	string Group { get; }
	bool Hide { get; }
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
