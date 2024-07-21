using System;
using System.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

using Kaizerwald.Utilities;
using NUnit.Framework.Internal;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using static Kaizerwald.Utilities.Core.KzwMath;

namespace Kaizerwald.TerrainBuilder
{
    // Vertices Order
    // 2 ━━━ 3
    // ┃     ┃
    // 0 ━━━ 1
    public readonly struct Cell
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public readonly int2 Coords;
        public readonly FixedList64Bytes<float3> Vertices;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Cell(int2 coords, NativeArray<float3> cellVertices)
        {
            Coords = coords;
            Vertices = new FixedList64Bytes<float3>();
            for (int i = 0; i < cellVertices.Length; i++) Vertices.Add(cellVertices[i]);
        }

        public Cell(int x, int y, NativeArray<float3> cellVertices) : this(new int2(x, y), cellVertices) { }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                               ◆◆◆◆◆◆ METHODS ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //IMPORTANT: vertices use must be the ones slicing the quads in 2 triangles
        public readonly float3 Center => Vertices[1] + normalize(Vertices[2] - Vertices[1]) * (SQRT2 / 2);
        public readonly float HighestPoint => cmax(float4(Vertices[0].y, Vertices[1].y, Vertices[2].y, Vertices[3].y));

        // Normals
        public readonly float3 NormalTriangleLeft => normalize(cross(Vertices[2] - Vertices[0], Vertices[1] - Vertices[0]));
        public readonly float3 NormalTriangleRight => normalize(cross(Vertices[1] - Vertices[3], Vertices[2] - Vertices[3]));
        
        
        public readonly bool IsInside(float2 position2D)
        {
            float4 xComponents = float4(Vertices[0].x, Vertices[1].x, Vertices[2].x, Vertices[3].x);
            float4 zComponents = float4(Vertices[0].z, Vertices[1].z, Vertices[2].z, Vertices[3].z);
            float2 minMaxX = new (cmin(xComponents), cmax(xComponents));
            float2 minMaxY = new (cmin(zComponents), cmax(zComponents));
            // Check if the position is within the AABB
            return position2D.x >= minMaxX[0] && position2D.x <= minMaxX[1] && position2D.y >= minMaxY[0] && position2D.y <= minMaxY[1];
        }
        public readonly bool IsInside(float3 position) => IsInside(position.xz);
        
        public readonly bool IsInLeftTriangle(float2 position2D)
        {
            float distanceFromLeft = distancesq(position2D, Vertices[0].xz);
            float distanceFromRight = distancesq(position2D, Vertices[3].xz);
            return distanceFromLeft < distanceFromRight;
        }
        
        public readonly float3 GetPosition(float2 position2D)
        {
            //bool isLeftTri = IsPointInTriangle(LeftTriangle, position2D);
            bool isLeftTri = IsInLeftTriangle(position2D);
            float3 rayOrigin = new (position2D.x, ceil(HighestPoint), position2D.y);
            float3 triangleNormal = isLeftTri ? NormalTriangleLeft : NormalTriangleRight;
            
            //Point A : start
            float3 a = isLeftTri ? Vertices[0] : Vertices[1];
            float t = dot(a - rayOrigin, triangleNormal) / dot(down(), triangleNormal);
            return mad(t,down(), rayOrigin);
        }
        
        public readonly float3 GetPosition(float3 position)
        {
            return GetPosition(position.xz);
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Overrides ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public readonly override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < Vertices.Length; i++)
            {
                stringBuilder.Append($"Vertex({i}) = {Vertices[i]}, ");
            }
            stringBuilder.Append($"Coords: {Coords}; Center: {Center}");
            return stringBuilder.ToString();
        }
    }
}
