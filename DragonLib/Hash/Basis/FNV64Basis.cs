namespace DragonLib.Hash.Basis;

// https://tools.ietf.org/html/draft-eastlake-fnv-17
// http://www.isthe.com/chongo/tech/comp/fnv/index.html
public enum FNV64Basis : ulong {
    Default = FNV1,
    FNV0 = 0xdf8e50cb9ed4bbfe, // chongo (Landon Curt Noll) /\oo/\
    FNV1 = 0xcbf29ce484222325, // chongo <Landon Curt Noll> /\../\
}
