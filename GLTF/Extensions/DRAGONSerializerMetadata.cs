namespace DragonLib.GLTF.Extensions
{
    public class DRAGONSerializerMetadata : GLTFProperty, IGLTFExtension
    {
        public const string Identifier = "_DRAGON_SerializerMetadata";
        
        public string DragonLib { get; set; }
        public string Project { get; set; }
        
        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFRoot)) return;
            gltf.Extensions[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
        }
    }
}
