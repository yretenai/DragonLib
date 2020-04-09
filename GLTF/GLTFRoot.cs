using System.Collections.Generic;
using DragonLib.GLTF.Converters;
using DragonLib.GLTF.Extensions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DragonLib.GLTF
{
    [PublicAPI]
    public class GLTFRoot : GLTFProperty
    {
        private static JsonSerializerSettings GLTFSettings = new JsonSerializerSettings
        {
            ContractResolver = new IgnoreEmptyContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Include,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Converters =
            {
                new HalfConverter(),
                new Matrix3x3Converter(),
                new Matrix4x3Converter(),
                new Matrix4x4Converter(),
                new QuaternionConverter(),
                new Vector2Converter(),
                new Vector3Converter(),
                new Vector4Converter()
            }
        };

        public GLTFAsset? Asset { get; set; }
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

        public static GLTFRoot? Deserialize(string data) => JsonConvert.DeserializeObject<GLTFRoot>(data, GLTFSettings);

        public string Serialize(string project)
        {
            new DRAGONSerializerMetadata
            {
                DragonLib = "GLTF Version 2.0 - DragonLib Implementation - NET Core 3",
                Project = project
            }.Insert(this, this);

            return JsonConvert.SerializeObject(this, GLTFSettings);
        }
    }
}
