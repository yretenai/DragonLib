using System.Text;
using DragonLib.Hash;
using DragonLib.Hash.Basis;
using DragonLib.Hash.Algorithms;

namespace DragonLib.Tests.Hash;

public class FNVTests {
    [TestCase(FNVAlgorithm<uint>.FNV1_IV, 0x811c9dc5u), TestCase(FNVAlgorithm<uint>.FNV1B_IV, 0x8d9a085eu)]
    public void FnvTestBasis32(string iv, uint check) {
        var basis = FNVAlgorithm<uint>.CalculateBasis(iv, 0x01000193u);
        Assert.That(basis, Is.EqualTo(check));
    }

    [TestCase(FNVAlgorithm<uint>.FNV1_IV, 0xcbf29ce484222325ul), TestCase(FNVAlgorithm<uint>.FNV1B_IV, 0xdf8e50cb9ed4bbfeul)]
    public void FnvTestBasis64(string iv, ulong check) {
        var basis = FNVAlgorithm<ulong>.CalculateBasis(iv, 0x00000100000001B3ul);
        Assert.That(basis, Is.EqualTo(check));
    }

    [TestCase(FNV32Basis.FNV0, "chongo was here!", 0x007c6c4cu), TestCase(FNV32Basis.FNV1, "chongo was here!", 0xb10d5725u)]
    public void FnvTest32(FNV32Basis basis, string test, uint check) {
        using var fnv = FowlerNollVo.Create(basis);
        Assert.That(fnv.ComputeHash(Encoding.ASCII.GetBytes(test)), Is.EqualTo(BitConverter.GetBytes(check)));
        Assert.That(fnv.ComputeHashValue(Encoding.ASCII.GetBytes(test)), Is.EqualTo(check));
    }

    [TestCase(FNV32Basis.FNV1, "chongo was here!", 0x448524fdu)]
    public void FnvAltTest32(FNV32Basis basis, string test, uint check) {
        using var fnv = FowlerNollVo.CreateAlternate(basis);
        Assert.That(fnv.ComputeHash(Encoding.ASCII.GetBytes(test)), Is.EqualTo(BitConverter.GetBytes(check)));
        Assert.That(fnv.ComputeHashValue(Encoding.ASCII.GetBytes(test)), Is.EqualTo(check));
    }

    [TestCase(FNV64Basis.FNV0, "chongo was here!", 0x4a7c4c49fb224d0cul), TestCase(FNV64Basis.FNV1, "chongo was here!", 0x4c9ca59581b27f45ul)]
    public void FnvTest64(FNV64Basis basis, string test, ulong check) {
        using var fnv = FowlerNollVo.Create(basis);
        Assert.That(fnv.ComputeHash(Encoding.ASCII.GetBytes(test)), Is.EqualTo(BitConverter.GetBytes(check)));
        Assert.That(fnv.ComputeHashValue(Encoding.ASCII.GetBytes(test)), Is.EqualTo(check));
    }

    [TestCase(FNV64Basis.FNV1, "chongo was here!", 0x858e2fa32a55e61dul)]
    public void FnvAltTest64(FNV64Basis basis, string test, ulong check) {
        using var fnv = FowlerNollVo.CreateAlternate(basis);
        Assert.That(fnv.ComputeHash(Encoding.ASCII.GetBytes(test)), Is.EqualTo(BitConverter.GetBytes(check)));
        Assert.That(fnv.ComputeHashValue(Encoding.ASCII.GetBytes(test)), Is.EqualTo(check));
    }
}
