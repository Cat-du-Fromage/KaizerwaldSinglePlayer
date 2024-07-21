using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine.Rendering;
using UnityEngine;

using Kaizerwald.Pattern;
using Kaizerwald.Utilities.Core;

using static Unity.Mathematics.math;

using int2 = Unity.Mathematics.int2;

namespace Kaizerwald
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    public class TerrainManager : Singleton<TerrainManager>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ DEBUG ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [SerializeField] private bool DebugStartsSpawners;
        private readonly Color[] colors = { Color.green, Color.red, Color.cyan, Color.yellow };
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ Const/ReadOnly ◆◆◆◆◆◆                                            ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private enum ECardinal : int
        {
            North = 0,
            South = 1,
            East = 2,
            West = 3
        }
        
        private const int SPAWNER_DEPTH_SIZE = 16;
        private const int BORDER_OFFSET = 16;

        private readonly Vector3 spawnerOffset = new Vector3(0.1f, 1, 0.1f);
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [SerializeField] private Material DefaultMaterial;
        [SerializeField] private List<GameObject> PlayerSpawns;
        
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [field:SerializeField] public Transform TerrainTemplate { get; private set; }
        [field:SerializeField] public int2 SizeXY { get; private set; }
        
        public Transform TerrainTransform { get; private set; }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        protected override void OnAwake()
        {
            base.OnAwake();
            TerrainTransform = transform;
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
            
            CreateTerrain();
            InitializeSpawners();
        }

#if UNITY_EDITOR
        // ================================================ Debug ======================================================
        private void OnDrawGizmos()
        {
            if (!DebugStartsSpawners || PlayerSpawns == null || PlayerSpawns.Count == 0) return;
            for (int i = 0; i < PlayerSpawns.Count; i++)
            {
                Gizmos.color = colors[i];
                Vector3 position = GetPlayerFirstSpawnPosition(i);
                Gizmos.DrawSphere(position, 2f);
            }
        }
        // ================================================ Debug ======================================================
#endif
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private Vector3 GetDirection(ECardinal direction)
        {
            return direction switch
            {
                ECardinal.North => Vector3.forward,
                ECardinal.South => Vector3.back,
                ECardinal.East  => Vector3.right,
                ECardinal.West  => Vector3.left,
                _ => Vector3.zero,
            }; 
        }
        
        private void CreateTerrain()
        {
            // Get Terrain Size
            SizeXY = max(1, TerrainTemplate == null ? SizeXY : (int2)(((float3)TerrainTemplate.transform.localScale).xz * 10f));
            transform.localScale = Vector3.one;
            
            // Create and Assign Terrain Mesh
            Mesh terrainMesh = MeshUtils.CreatePlaneMesh(SizeXY);
            meshFilter.sharedMesh = terrainMesh;
            meshCollider.sharedMesh = terrainMesh;
            
            // Terrain Material override
            if (DefaultMaterial != null) meshRenderer.sharedMaterial = DefaultMaterial;
        }

        private void InitializeSpawners()
        {
            PlayerSpawns = new List<GameObject>(TerrainTransform.childCount);
            for (int i = 0; i < TerrainTransform.childCount; i++)
            {
                Transform child = TerrainTransform.GetChild(i);
                child.forward = -GetDirection((ECardinal)i); // Use inverse direction, because we wont dir to center
                child.position = GetSpawnerPosition((ECardinal)i);
                child.localScale = Vector3.Scale(spawnerOffset, new Vector3(SizeXY.x - BORDER_OFFSET, 1, SPAWNER_DEPTH_SIZE));
                PlayerSpawns.Add(child.gameObject);
            }
        }
        
        private Vector3 GetSpawnerPosition(ECardinal direction)
        {
            Vector2 halfSizeXY = (float2)SizeXY / 2f;
            Vector3 dirOffset = GetDirection(direction);
            
            Vector3 offset = dirOffset * (dirOffset.x == 0 ? halfSizeXY.y : halfSizeXY.x);
            offset -= dirOffset * (SPAWNER_DEPTH_SIZE / 2f + BORDER_OFFSET);
            offset += Vector3.up * 0.01f;
            
            return offset;
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Spawner ◈◈◈◈◈◈                                                                                      ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public GameObject GetSpawnerForNumber(int index)
        {
            return index < 0 || index >= PlayerSpawns.Count ? null : PlayerSpawns[index];
        }

        public Transform GetSpawnerTransform(int spawnIndex)
        {
            return GetSpawnerForNumber(spawnIndex).transform;
        }
        
        public Vector3 GetTeamCenterSpawnPosition(int spawnIndex)
        {
            Transform spawnerTransform = GetSpawnerTransform(spawnIndex);
            return spawnerTransform == null ? Vector3.zero : Vector3.Scale(spawnerTransform.position, new Vector3(1f,0,1f));
        }
        
        public Vector3 GetPlayerFirstSpawnPosition(int spawnIndex)
        {
            Transform spawnerTransform = GetSpawnerTransform(spawnIndex);
            if(spawnerTransform == null) return Vector3.zero;
            
            float spawnHorizontalSize = (SizeXY.x / 2f) - BORDER_OFFSET;
            Vector3 left = -spawnerTransform.right;
            Vector3 firstSpawnPoint = spawnerTransform.position + left * spawnHorizontalSize;
            return Vector3.Scale(firstSpawnPoint, new Vector3(1f,0,1f));
        }
    }
}
