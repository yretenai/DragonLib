using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace DragonLib.GLTF
{
    [PublicAPI, JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GLTFAccessorAttributeType
    {
        [EnumMember(Value = "SCALAR")]
        Scalar,
        [EnumMember(Value = "VEC2")]
        Vector2,
        [EnumMember(Value = "VEC3")]
        Vector3,
        [EnumMember(Value = "VEC4")]
        Vector4,
        [EnumMember(Value = "MAT2")]
        Matrix2,
        [EnumMember(Value = "MAT3")]
        Matrix3,
        [EnumMember(Value = "MAT4")]
        Matrix4
    }
}
