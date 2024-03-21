using System.Reflection;
using System.Text.Json.Serialization;

namespace DragonLib.CommandLine;

[AttributeUsage(AttributeTargets.Property)]
public sealed class FlagAttribute(string flag) : Attribute {
	[JsonIgnore] public string Flag { get; set; } = flag;
	public string? Help { get; set; }
	public string? Category { get; set; }
	public string? Visitor { get; set; }
	public bool Hidden { get; set; }
	[JsonIgnore] public Assembly? VisitorAssembly { get; set; }
	public bool IsRequired { get; set; }
	public int Positional { get; set; } = -1;
	public string[]? ValidValues { get; set; } = [];
	[JsonIgnore] public string[]? Aliases { get; set; } = [];
	public string[]? EnumPrefix { get; set; }
	public char ReplaceDashes { get; set; }
	public char ReplaceDots { get; set; }
	public object? Extra { get; set; }
	public string[] Flags => Aliases?.Concat([Flag]).Distinct().Reverse().ToArray() ?? [Flag];
	[JsonIgnore] public override object TypeId => Flag;

	public override bool Equals(object? obj) {
		if (obj is FlagAttribute attribute) {
			return Equals(attribute);
		}

		return base.Equals(obj);
	}

	public bool Equals(FlagAttribute other) =>
		base.Equals(other) && Flag == other.Flag && Help == other.Help && Category == other.Category &&
		Visitor == other.Visitor && Hidden == other.Hidden && VisitorAssembly == other.VisitorAssembly &&
		IsRequired == other.IsRequired && Positional == other.Positional &&
		ValidValues?.Equals(other.ValidValues) == true &&
		Aliases?.Equals(other.Aliases) == true && EnumPrefix?.Equals(other.EnumPrefix) == true &&
		ReplaceDashes.Equals(other.ReplaceDashes);

	public override string ToString() => $"-{(Flag.Length > 1 ? "-" : string.Empty)}{Flag}: {Help}";

	public override bool Match(object? obj) {
		if (obj is not FlagAttribute attribute) {
			return false;
		}

		var otherFlags = attribute.Flags;
		return Flags.Any(flag => otherFlags.Contains(flag));
	}

	public override bool IsDefaultAttribute() => string.IsNullOrWhiteSpace(Flag);

	public override int GetHashCode() =>
		HashCode.Combine(base.GetHashCode(),
		                 Flag,
		                 HashCode.Combine(Help, Category, Visitor, VisitorAssembly?.GetHashCode() ?? 0, Hidden),
		                 IsRequired,
		                 Positional,
		                 HashCode.Combine(ValidValues, Aliases, EnumPrefix, ReplaceDashes));
}
