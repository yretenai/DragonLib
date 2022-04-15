namespace DragonLib.Hash.Basis;

public record CRCVariant<T>(T Polynomial, T Init, T Xor, bool ReflectIn, bool ReflectOut);
