namespace DragonLib.Hash.Basis;

public record struct CRCVariant<T>(T Polynomial, T Init, T Xor, bool ReflectIn, bool ReflectOut);
