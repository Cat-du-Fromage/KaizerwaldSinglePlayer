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

using Kaizerwald.Pattern;
using Kaizerwald.Utilities.Core;

namespace Kaizerwald.TerrainBuilder
{
    [RequireComponent(typeof(SimpleTerrain))]
    [ExecuteAfter(typeof(SimpleTerrain), OrderIncrease = 1)]
    public class TerrainGridSystem : SingletonBehaviour<TerrainGridSystem>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [field:SerializeField] private SimpleTerrain SimpleTerrain;
        [field: SerializeField] private TerrainSettings TerrainSettings;

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

        private void OnDestroy()
        {
            OnDispose();
        }

        public void OnDispose()
        {
            if (nodes.IsCreated) nodes.Dispose();
            if (obstacles.IsCreated) obstacles.Dispose();
        }
        
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || GridCells.Cells == null) return;
            Gizmos.color = Color.green;
            for (int i = 0; i < GridCells.Count; i++)
            {
                Vector3 center = GridCells.Cells[i].Center;
                Gizmos.DrawWireCube(center, new float3(0.8f));
            }
        }
#endif
        

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected override void Initialize()
        {
            SimpleTerrain = GetComponent<SimpleTerrain>();
            TerrainSettings = GetComponent<TerrainSettings>();
            
            print($"setting null ? {SimpleTerrain.TerrainSettings == null}");
            OnDispose();
            gridCells = new GridCells(SimpleTerrain);
            nodes = new NativeArray<Node>(SimpleTerrain.TerrainSettings.QuadCount, Persistent, UninitializedMemory);
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new Node(i, SimpleTerrain.TerrainSettings.NumQuadX);
            }
        }
        /*
        public TerrainGridSystem Initialize(SimpleTerrain terrain)
        {
            OnDispose();
            gridCells = new GridCells(terrain);
            nodes = new NativeArray<Node>(terrain.TerrainSettings.QuadCount, Persistent, UninitializedMemory);
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new Node(i, terrain.TerrainSettings.NumQuadX);
            }
            return this;
        }
        */
        public NativeList<int> GetPathTo(float3 currentPosition, float3 targetPosition)
        {
            NativeList<int> pathList = new (8, TempJob);
            int startIndex = KzwGrid.GetIndexFromPositionCentered(currentPosition.xz, gridCells.NumCellXY);
            int endIndex = KzwGrid.GetIndexFromPositionCentered(targetPosition.xz, gridCells.NumCellXY);
            JAStar.Schedule(pathList, gridCells.NumCellXY, startIndex, endIndex, nodes, true).Complete();
            return pathList;
        }
        
        public NativeArray<Cell> GetCellPathTo(float3 currentPosition, float3 targetPosition, Allocator allocator = Temp)
        {
            using NativeList<int> pathList = GetPathTo(currentPosition, targetPosition);
            
            NativeArray<Cell> cellPath = new(pathList.Length, allocator, UninitializedMemory);
            for (int i = 0; i < pathList.Length; i++)
            {
                cellPath[i] = gridCells.Cells[pathList[i]];
            }
            return cellPath;
        }
    }
}
