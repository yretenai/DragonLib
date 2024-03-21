using System.Diagnostics.CodeAnalysis;
using DragonLib.Indent;

namespace DragonLib.Xml;

public interface IDragonMarkupSerializer {
	DragonMarkupType OverrideTarget { get; }

	[SuppressMessage("ReSharper", "UnusedParameter.Global")]
	string? Print(object? instance, Dictionary<object, int> visited, IndentHelperBase indents, string? valueName, DragonMarkupSettings settings);
}

public interface IDragonMarkupSerializerFactory {
	IDragonMarkupSerializer? GetSerializer(Type type);
}
