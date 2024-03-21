// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace DragonLib.Xml;

public sealed record DragonMarkupSettings {
	public List<IDragonMarkupSerializerFactory> TypeFactories = new();
	public Dictionary<Type, IDragonMarkupSerializer> TypeSerializers = new();

	public bool UseRefId { get; init; } = true;

	public bool WriteXmlHeader { get; init; } = true;
	public bool WriteFields { get; init; } = true;
	public bool WriteProperties { get; init; } = true;

	public string Namespace { get; init; } = "dragon";

	public Dictionary<string, string> Namespaces { get; init; } = new() {
		{ "dragon", "https://ns.vore.ink/dml/v1" },
	};

	public static DragonMarkupSettings Default => new();

	public static DragonMarkupSettings Slim =>
		Default with {
			UseRefId = false,
		};
}
