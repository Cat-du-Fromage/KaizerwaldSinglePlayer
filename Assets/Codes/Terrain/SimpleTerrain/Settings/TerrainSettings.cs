using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
    
using static Unity.Mathematics.math;

namespace Kaizerwald.TerrainBuilder
{
    public class TerrainSettings : MonoBehaviour
    {
        // Size = Num Quads
        [field: SerializeField] public int SizeX { get; private set; }
        [field: SerializeField] public int SizeY { get; private set; }

        public int2 SizeXY => new int2(SizeX, SizeY);
        
        //QUAD (= Size)
        public int NumQuadX => SizeX;
        public int NumQuadY => SizeY;
        
        public int2 NumQuadsXY => SizeXY;
        
        public int QuadCount => NumQuadX * NumQuadY;
        
        //VERTEX
        public int NumVerticesX => SizeX + 1;
        public int NumVerticesY => SizeY + 1;
        
        public int2 NumVerticesXY => SizeXY + 1;
        
        public int VerticesCount => NumVerticesX * NumVerticesY;
        
        //Triangles
        public int TrianglesCount => QuadCount * 2;
        public int TriangleIndicesCount => QuadCount * 6;
        
        private void OnEnable()
        {
            SizeX = max(1, ceilpow2(SizeX));
            SizeY = max(1, ceilpow2(SizeY));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            SizeX = max(1, ceilpow2(SizeX));
            SizeY = max(1, ceilpow2(SizeY));
        }
#endif
    }
}
