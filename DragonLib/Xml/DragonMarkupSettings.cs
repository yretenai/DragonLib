// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace DragonLib.Xml;

public class DragonMarkupSettings {
    public Dictionary<Type, IDragonMarkupSerializer> TypeSerializers = new();

    public bool UseRefId { get; init; } = true;

    public bool WriteXmlHeader { get; init; } = true;

    public string Namespace { get; init; } = "dragon";

    public Dictionary<string, string> Namespaces { get; init; } = new() {
        { "dragon", "https://legiayayana.com/dml/v1" },
    };

    public static DragonMarkupSettings Default => new();

    public static DragonMarkupSettings Slim =>
        new() {
            UseRefId = false,
        };
}
