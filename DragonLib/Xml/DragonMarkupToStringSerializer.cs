using DragonLib.Indent;

namespace DragonLib.Xml;

public class DragonMarkupToStringSerializer : IDragonMarkupSerializer {
	public static readonly IDragonMarkupSerializer Default = new DragonMarkupToStringSerializer();

	public DragonMarkupType OverrideTarget => DragonMarkupType.Value;

	public string? Print(object? instance, Dictionary<object, int> visited, IndentHelperBase indent, string? name, DragonMarkupSettings settings) => instance?.ToString();
}
