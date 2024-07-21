using System;

namespace Kaizerwald.FieldOfView
{
    [Serializable]
    public struct MeshSectionInfos
    {
        public float OuterStep;
        public float InnerStep;

        public int QuadCount;
        public int VertexCount;
        public readonly int TriangleCount => QuadCount * 2;
        public readonly int TriangleIndicesCount => TriangleCount * 3;

        public readonly override string ToString()
        {
            return $"QuadCount = {QuadCount} | VertexCount = {VertexCount} | OuterStep = {OuterStep} | InnerStep = {InnerStep}";
        }
    }
}