using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.InputSystem;

using static Unity.Mathematics.math;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using float2 = Unity.Mathematics.float2;
using float3 = Unity.Mathematics.float3;

using Kaizerwald.Utilities;

namespace Kaizerwald.TerrainBuilder
{
    [RequireComponent(typeof(SimpleTerrain))]
    public class TerrainGridSystem : Singleton<TerrainGridSystem>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private NativeArray<Node> nodes; //MUST NEVER BE CHANGED!
        private NativeArray<bool> obstacles;

        private GridCells gridCells;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public GridCells GridCells => gridCells;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected override void OnAwake()
        {
            base.OnAwake();
        }

        private void OnDestroy()
        {
            if (nodes.IsCreated) nodes.Dispose();
            if (obstacles.IsCreated) obstacles.Dispose();
        }
        
        /*
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Terrain == null) return;
            if (!Terrain.IsInitialized || GridCells.Cells == null) return;
            Gizmos.color = Color.green;

            for (int i = 0; i < GridCells.Count; i++)
            {
                Vector3 center = GridCells.Cells[i].Center;
                Gizmos.DrawWireCube(center, new float3(0.8f));
            }
        }
#endif
        */

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void Initialize(SimpleTerrain terrain)
        {
            gridCells = new GridCells(terrain);
            nodes = new NativeArray<Node>(terrain.TerrainSettings.QuadCount, Persistent, UninitializedMemory);
            for (int i = 0; i < nodes.Length; i++) nodes[i] = new Node(i, terrain.TerrainSettings.NumQuadX);
        }

        public NativeList<int> GetPathTo(float3 currentPosition, float3 targetPosition)
        {
            NativeList<int> pathList = new (8, Allocator.TempJob);
            int startIndex = KzwGrid.GetIndexFromPositionCentered(currentPosition.xz, gridCells.NumCellXY);
            int endIndex = KzwGrid.GetIndexFromPositionCentered(targetPosition.xz, gridCells.NumCellXY);
            JAStar.Process(pathList, gridCells.NumCellXY, startIndex, endIndex, nodes, true).Complete();
            return pathList;
        }
        
        public NativeArray<Cell> GetCellPathTo(float3 currentPosition, float3 targetPosition, Allocator allocator = Temp)
        {
            NativeList<int> pathList = new (8, Allocator.TempJob);
            int startIndex = KzwGrid.GetIndexFromPositionCentered(currentPosition.xz, gridCells.NumCellXY);
            int endIndex = KzwGrid.GetIndexFromPositionCentered(targetPosition.xz, gridCells.NumCellXY);
            JAStar.Process(pathList, gridCells.NumCellXY, startIndex, endIndex, nodes, true).Complete();
            
            NativeArray<Cell> cellPath = new(pathList.Length, allocator, UninitializedMemory);
            for (int i = 0; i < pathList.Length; i++) cellPath[i] = gridCells.Cells[pathList[i]];
            
            pathList.Dispose();
            return cellPath;
        }
    }
}
