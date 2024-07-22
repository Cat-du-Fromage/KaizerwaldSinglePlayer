using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static Unity.Mathematics.noise;
using static UnityEngine.Mesh;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using Kaizerwald.Utilities.Core;
using static Kaizerwald.Utilities.Core.KzwMath;
using static Kaizerwald.Utilities.Core.KzwGrid;

namespace Kaizerwald.TerrainBuilder
{
    public class GridCells
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public int2 NumCellXY { get; private set; }
        public Cell[] Cells { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public int Count => Cells.Length;
        public Cell this[int index] => Cells[index];
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public GridCells(SimpleTerrain simpleTerrain)
        {
            NumCellXY = simpleTerrain.TerrainSettings.NumQuadsXY;
            CreateGridCells(simpleTerrain);
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void CreateGridCells(SimpleTerrain terrain)
        {
            const int numVertexPerQuad = 4;
            const int quadWidth = 2;
            
            TerrainSettings terrainSettings = terrain.TerrainSettings;
            
            using MeshDataArray meshDataArray = AcquireReadOnlyMeshData(terrain.TerrainMesh);
            
            Cells = new Cell[terrainSettings.QuadCount];
            using NativeArray<float3> verticesNtv = meshDataArray[0].GetVertexData<float3>(stream: 0);
            
            NativeArray<float3> cellVertices = new (4, Temp, UninitializedMemory);
            for (int cellIndex = 0; cellIndex < Cells.Length; cellIndex++)
            {
                int2 cellCoords = GetXY2(cellIndex, terrainSettings.NumQuadX);
                for (int vertexIndex = 0; vertexIndex < numVertexPerQuad; vertexIndex++)
                {
                    int2 vertexCoords = GetXY2(vertexIndex, quadWidth);
                    int index = GetIndex(cellCoords + vertexCoords, terrainSettings.NumVerticesX);
                    cellVertices[vertexIndex] = verticesNtv[index];
                }
                Cells[cellIndex] = new Cell(cellCoords, cellVertices);
            }
        }
        //Debug.Log($"verticesNtv len = {verticesNtv.Length}");
        //Debug.Log($"Cells.Length = {Cells.Length}, sizeX = {terrain.TerrainSettings.SizeX}, sizeY = {terrain.TerrainSettings.SizeY}, mul = {terrain.TerrainSettings.SizeX * terrain.TerrainSettings.SizeY}");
    }
}