using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public interface IGLTFExtension
    {
        public void Insert(GLTFProperty gltf, GLTFRoot root);
    }
}
