using System;
using Unity.Mathematics;
using UnityEngine.Rendering;

using static Unity.Mathematics.math;

namespace Kaizerwald.FieldOfView
{
    [Serializable]
    public struct MeshInfos
    {
        public MeshSectionInfos Border;
        public MeshSectionInfos Arc;
        public MeshSectionInfos Front;

        // General Infos
        public readonly int VerticesCount => Border.VertexCount + Arc.VertexCount + Front.VertexCount;
        public readonly int TriangleCount => Border.TriangleCount + Arc.TriangleCount + Front.TriangleCount;
        public readonly int TriangleIndicesCount => Border.TriangleIndicesCount + Arc.TriangleIndicesCount + Front.TriangleIndicesCount;

        public MeshInfos(float range, float sideAngleRadian, float widthLength, float thickness, float resolution = 1)
        {
            // Border
            int oneSideBorderQuadCount = (int)round(range);
            Border = new MeshSectionInfos
            {
                OuterStep   = range / oneSideBorderQuadCount,
                InnerStep   = (range - thickness) / oneSideBorderQuadCount,
                QuadCount   = oneSideBorderQuadCount * 2,
                VertexCount = 2 * (oneSideBorderQuadCount + 1) * 2
            };

            // Arc
            int oneSideArcQuadCount = (int)max(round((PIHALF - sideAngleRadian) * range), 0);
            sincos(sideAngleRadian, out float sinA, out float cosA);
            float2 outerBorderStart = float2(widthLength / 2, 0);
            float2 innerBorderStart = outerBorderStart + float2(-sinA, cosA) * thickness;
            float2 borderInnerEnd   = innerBorderStart + float2(cosA, sinA) * (range - thickness);
            float2 borderOuterStartToInnerEnd = normalize(borderInnerEnd - outerBorderStart);
            float innerSideAngleRadian = PIHALF - atan2(borderOuterStartToInnerEnd.y, borderOuterStartToInnerEnd.x);
            Arc = new MeshSectionInfos
            {
                OuterStep   = (PIHALF - sideAngleRadian) / oneSideArcQuadCount,
                InnerStep   = innerSideAngleRadian / oneSideArcQuadCount,
                QuadCount   = oneSideArcQuadCount * 2,
                VertexCount = max(0, 2 * (oneSideArcQuadCount + 1) - 4) * 2
            };

            // Front
            //we don't separate in 2 parts for the front
            int frontLineQuadCount = (int)max(1, round(widthLength));
            Front = new MeshSectionInfos
            {
                OuterStep = widthLength / frontLineQuadCount,
                InnerStep = widthLength / frontLineQuadCount,
                QuadCount = frontLineQuadCount,
                VertexCount = (frontLineQuadCount + 1) * 2
            };
        }

        public MeshInfos(FieldOfViewParams fovParams, float thickness, float resolution = 1) 
            : this(fovParams.Range, fovParams.SideAngleRadian, fovParams.WidthLength, thickness, resolution)
        {
            
        }
        
        public MeshInfos(FieldOfViewParams fovParams, float widthLength, float thickness, float resolution = 1) 
            : this(fovParams.Range, fovParams.SideAngleRadian, widthLength, thickness, resolution)
        {
            
        }
    }
}