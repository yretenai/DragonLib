using System.Collections.Generic;
using System.IO;
using DragonLib.Numerics;
using JetBrains.Annotations;
using static DragonLib.OWM.OWMHelper;

namespace DragonLib.OWM
{
    [PublicAPI]
    public class OWMDL
    {
        private const short VERSION_MAJOR = 0x1;
        private const short VERSION_MINOR = 0x6;

        public OWMDL(string name = "")
        {
            Name = name;
        }

        public string Name { get; set; }
        public string? MaterialLib { get; set; }
        public List<OWMDLBone> Bones { get; set; } = new List<OWMDLBone>();
        public List<OWMDLMesh> Meshes { get; set; } = new List<OWMDLMesh>();
        public List<OWMDLSocket> Sockets { get; set; } = new List<OWMDLSocket>();
        public List<OWMDLRefBone> RestPose { get; set; } = new List<OWMDLRefBone>();
        public uint GUID { get; set; }

        public Stream Write()
        {
            var stream = new MemoryStream();

            // Version 1.6

            stream.Write(GetBytes(VERSION_MAJOR, VERSION_MINOR));
            stream.Write(GetString(Name));
            stream.Write(GetString(MaterialLib));
            stream.Write(GetBytes((short) Bones.Count));
            stream.Write(GetBytes(Meshes.Count, Sockets.Count));

            for (var index = 0; index < Bones.Count; index++)
            {
                var bone = Bones[index];
                stream.Write(GetString(bone.Name));
                stream.Write(GetBytes(bone.Parent == -1 ? (short) index : bone.Parent));
                stream.Write(GetBytes(bone.Position, bone.Scale));
                stream.Write(GetBytes(bone.Rotation));
            }

            foreach (var mesh in Meshes)
            {
                stream.Write(GetString(mesh.Name));
                stream.Write(GetBytes(mesh.MaterialId));
                stream.Write(GetBytes(mesh.UVCount));
                stream.Write(GetBytes(mesh.Vertices.Count));
                stream.Write(GetBytes(mesh.Faces.Count));

                foreach (var vertex in mesh.Vertices)
                {
                    stream.Write(GetBytes(vertex.Position, vertex.Normal));
                    stream.Write(GetBytes(vertex.TexCoords));
                    stream.Write(GetBytes((byte) vertex.Joints.Length));
                    stream.Write(GetBytes(vertex.Joints));
                    stream.Write(GetBytes(vertex.Weights));
                    stream.Write(GetBytes(vertex.Color1));
                    stream.Write(GetBytes(vertex.Color2));
                }

                foreach (var face in mesh.Faces)
                {
                    stream.Write(GetBytes((byte) face.Length));
                    stream.Write(GetBytes(face));
                }
            }

            foreach (var socket in Sockets)
            {
                stream.Write(GetString(socket.Name));
                stream.Write(GetBytes(socket.Position));
                stream.Write(GetBytes(socket.Rotation));
            }

            foreach (var socket in Sockets) stream.Write(GetString(socket.Bone));

            // Cloth Count
            stream.Write(GetBytes(0));

            foreach (var bone in RestPose)
            {
                stream.Write(GetString(bone.Name));
                stream.Write(GetBytes(bone.Parent));
                stream.Write(GetBytes(bone.Position, bone.Scale));
                stream.Write(GetBytes(bone.Rotation));
            }

            stream.Write(GetBytes(GUID));
            stream.Position = 0;
            return stream;
        }
    }

    [PublicAPI]
    public struct OWMDLSocket
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public string Bone { get; set; }
    }

    [PublicAPI]
    public struct OWMDLMesh
    {
        public string Name { get; set; }
        public ulong MaterialId { get; set; }
        public byte UVCount { get; set; }
        public List<OWMDLMeshVertex> Vertices { get; set; }
        public List<int[]> Faces { get; set; }
    }

    [PublicAPI]
    public struct OWMDLMeshVertex
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Vector2[] TexCoords { get; set; }
        public ushort[] Joints { get; set; }
        public float[] Weights { get; set; }
        public Vector4 Color1 { get; set; }
        public Vector4 Color2 { get; set; }
    }

    [PublicAPI]
    public struct OWMDLBone
    {
        public string Name { get; set; }
        public short Parent { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Scale { get; set; }
        public Quaternion Rotation { get; set; }
    }

    [PublicAPI]
    public struct OWMDLRefBone
    {
        public string Name { get; set; }
        public short Parent { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Rotation { get; set; }
    }
}
