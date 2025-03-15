using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;


[StructLayout(LayoutKind.Sequential)]
public struct ChunkVertex
{
    public Vector3 pos;
    public sbyte normalX;
    public sbyte normalY;
    public sbyte normalZ;
    public sbyte normalW;

    public sbyte tangentX;
    public sbyte tangentY;
    public sbyte tangentZ;
    public sbyte tangentW;
    public Vector2 uvPos;

    public ChunkVertex(Vector3 v3, Vector3 nor, Vector3 tan, Vector2 v2)
    {
        pos = v3;
        normalX = (sbyte)((nor.x) * 127f);
        normalY = (sbyte)((nor.y) * 127f);
        normalZ = (sbyte)((nor.z) * 127f);
        normalW = 127;

        tangentX = (sbyte)((tan.x) * 127f);
        tangentY = (sbyte)((tan.y) * 127f);
        tangentZ = (sbyte)((tan.z) * 127f);
        tangentW = 127;
        uvPos = v2;
    }
    public static NativeArray<VertexAttributeDescriptor> vertexAttributesDes = new NativeArray<VertexAttributeDescriptor>(
        new VertexAttributeDescriptor[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.SNorm8, 4),
            new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.SNorm8, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
        }, Allocator.Persistent);
}