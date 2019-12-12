using System.Collections.Generic;
using System.Text.Json;
using DragonLib.GLTF.Converters;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFRoot : GLTFProperty
    {
        public GLTFAsset Asset { get; set; }
        public int Scene { get; set; }
        public HashSet<string> ExtensionsUsed { get; set; } = new HashSet<string>();
        public HashSet<string> ExtensionsRequired { get; set; } = new HashSet<string>();
        public List<GLTFAccessor> Accessors { get; set; } = new List<GLTFAccessor>();
        public List<GLTFAnimation> Animations { get; set; } = new List<GLTFAnimation>();
        public List<GLTFBuffer> Buffers { get; set; } = new List<GLTFBuffer>();
        public List<GLTFBufferView> BufferViews { get; set; } = new List<GLTFBufferView>();
        public List<GLTFCamera> Cameras { get; set; } = new List<GLTFCamera>();
        public List<GLTFImage> Images { get; set; } = new List<GLTFImage>();
        public List<GLTFMaterial> Materials { get; set; } = new List<GLTFMaterial>();
        public List<GLTFMesh> Meshes { get; set; } = new List<GLTFMesh>();
        public List<GLTFNode> Nodes { get; set; } = new List<GLTFNode>();
        public List<GLTFSampler> Samplers { get; set; } = new List<GLTFSampler>();
        public List<GLTFScene> Scenes { get; set; } = new List<GLTFScene>();
        public List<GLTFSkin> Skins { get; set; } = new List<GLTFSkin>();
        public List<GLTFTexture> Textures { get; set; } = new List<GLTFTexture>();

        public static GLTFRoot Deserialize(string data)
        {
            return JsonSerializer.Deserialize<GLTFRoot>(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                Converters =
                {
                    new GLTFNumericsConverter()
                }
            });
        }

        public string Serialize()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true
            });
        }
    }
}
