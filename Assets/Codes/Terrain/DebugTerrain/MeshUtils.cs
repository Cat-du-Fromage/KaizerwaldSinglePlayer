using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kaizerwald
{
    public static class MeshUtils
    {
        private static int[] SquareTriangleIndices => new int[] { 0, 2, 1, 1, 2, 3 };
        
        private static Vector3[] CenteredSquareVertices(int2 scaleXY)
        {
            float2 cornersValue = new float2(0.5f) * scaleXY;
            return new[]
            {
                new Vector3(-cornersValue.x, 0, -cornersValue.y),
                new Vector3(cornersValue.x, 0, -cornersValue.y),
                new Vector3(-cornersValue.x, 0, cornersValue.y),
                new Vector3(cornersValue.x, 0, cornersValue.y),
            };
        }

        private static Vector2[] SquareUvs(int2 sizeXY)
        {
            Vector2 uvValues = new Vector2(1f / (sizeXY.x + 1), 1f / (sizeXY.y + 1));
            return new Vector2[]
            {
                Vector2.zero,
                new (uvValues.x, 0),
                new (0, uvValues.y),
                uvValues,
            };
        }
        
        public static Mesh CreatePlaneMesh(int2 sizeXY)
        {
            Mesh mesh = new Mesh
            {
                name = "TerrainMesh",
                indexFormat = IndexFormat.UInt16,
                subMeshCount = 0,
                vertices = CenteredSquareVertices(sizeXY),
                uv = SquareUvs(sizeXY),
                triangles = SquareTriangleIndices,
            };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals(MeshUpdateFlags.DontRecalculateBounds);
            mesh.RecalculateTangents(MeshUpdateFlags.DontRecalculateBounds);
            return mesh;
        }
    }
}
