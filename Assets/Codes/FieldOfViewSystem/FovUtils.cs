using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Rendering;
using UnityEngine;

using static UnityEngine.Mesh;
using static UnityEngine.Rendering.VertexAttribute;
using static UnityEngine.Rendering.VertexAttributeFormat;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

namespace Kaizerwald.FieldOfView
{
    internal static class FovUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float Determinant(float2 a, float2 b)
        {
            return a.x * b.y - a.y * b.x;
        }
        
        internal static NativeArray<VertexAttributeDescriptor> GetVertexAttribute()
        {
            NativeArray<VertexAttributeDescriptor> vertexAttributes = new(4, Temp, UninitializedMemory);
            vertexAttributes[0] = new VertexAttributeDescriptor(Position, Float32, dimension: 3, stream: 0);
            vertexAttributes[1] = new VertexAttributeDescriptor(Normal, Float32, dimension: 3, stream: 1);
            vertexAttributes[2] = new VertexAttributeDescriptor(Tangent, Float16, dimension: 4, stream: 2);
            vertexAttributes[3] = new VertexAttributeDescriptor(TexCoord0, Float16, dimension: 2, stream: 3);
            return vertexAttributes;
        }
    }
}
