// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace DragonLib.Xml;

public class DragonMarkupSettings {
    /// <summary>
    ///     Custom Type Serializers
    /// </summary>
    public Dictionary<Type, IDragonMarkupSerializer> TypeSerializers = new();

    /// <summary>
    ///     Use ids in dragon:ref tags.
    /// </summary>
    public bool UseRefId { get; set; } = true;

    /// <summary>
    ///     Writes the ?xml header
    /// </summary>
    public bool WriteXmlHeader { get; set; } = true;

    /// <summary>
    ///     Prefix namespace for system tags
    /// </summary>
    public string Namespace { get; set; } = "dragon";

    /// <summary>
    ///     Dictionary of XML namespaces
    /// </summary>
    public Dictionary<string, string> Namespaces { get; set; } = new() {
        { "dragon", "https://legiayayana.com/dml/v1" },
    };

    public static DragonMarkupSettings Default => new();

    public static DragonMarkupSettings Slim =>
        new() {
            UseRefId = false,
        };
}
