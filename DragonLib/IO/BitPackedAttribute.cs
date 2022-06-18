namespace DragonLib.IO;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class BitPackedAttribute : Attribute {
    public BitPackedAttribute(Type baseType) => Type = baseType;

    public Type Type { get; }
}
