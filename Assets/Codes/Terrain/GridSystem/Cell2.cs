using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Kaizerwald.Utilities.Core;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Kaizerwald
{
    // Vertices Order
    // 2 ━━━ 3
    // ┃     ┃
    // 0 ━━━ 1
    public struct Cell2
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public readonly int Index;
        public readonly int Width;
        public half4 Heights;
        
        public readonly int2 Coords => KzwGrid.GetXY2(Index, Width);
        
        public readonly float HighestPoint => cmax(Heights);
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public Cell2(int2 coords, int width, NativeArray<float3> cellVertices)
        {
            Index = coords.y * width + coords.x;
            Width = width;
            Heights = new half4
            (
                (half)(cellVertices[0].y), 
                (half)(cellVertices[1].y), 
                (half)(cellVertices[2].y), 
                (half)(cellVertices[3].y)
            );
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                               ◆◆◆◆◆◆ METHODS ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public readonly float3 GetCenter()
        {
            float3 startVertex = VerticesAt(1);
            return startVertex + (VerticesAt(2) - startVertex) / 2;
        }
        
        // Normals
        public readonly float3 GetNormalTriangleLeft()
        {
            float3 startVertex = VerticesAt(0);
            return normalize(cross( VerticesAt(2) - startVertex,  VerticesAt(1) - startVertex));
        }

        public readonly float3 GetNormalTriangleRight()
        {
            float3 startVertex = VerticesAt(3);
            return normalize(cross(VerticesAt(1) - startVertex, VerticesAt(2) - startVertex));
        }

        public readonly float3 VerticesAt(int index)
        {
            //float offsetX = (index & 1) * cellSize; // If the least significant bit is 1, offsetX is cellSize, otherwise 0
            //float offsetZ = ((index >> 1) & 1) * cellSize; // If the second least significant bit is 1, offsetZ is cellSize, otherwise 0
            float2 vertexAPosition2D = Coords + int2(index & 1, (index >> 1) & 1);
            return new float3(vertexAPosition2D.x, Heights[index], vertexAPosition2D.y);
        }
        
        public bool IsInLeftTriangle(float2 position2D)
        {
            float distanceFromLeft = distancesq(position2D, VerticesAt(0).xz);
            float distanceFromRight = distancesq(position2D, VerticesAt(3).xz);
            return distanceFromLeft < distanceFromRight;
        }
        
        public float3 GetPosition(float2 position2D)
        {
            bool isLeftTri = IsInLeftTriangle(position2D);
            float3 rayOrigin = new (position2D.x, ceil(HighestPoint), position2D.y);
            float3 triangleNormal = isLeftTri ? GetNormalTriangleLeft() : GetNormalTriangleRight();
            
            //Point A : start
            //float3 a = isLeftTri ? VerticesAt(0) : VerticesAt(1);
            float3 start = VerticesAt(isLeftTri ? 0 : 1);
            float t = dot(start - rayOrigin, triangleNormal) / dot(down(), triangleNormal);
            return mad(t,down(), rayOrigin);
        }
    }
}
