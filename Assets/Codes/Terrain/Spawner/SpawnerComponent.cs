using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using static Unity.Mathematics.math;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

namespace Kaizerwald
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class SpawnerComponent : MonoBehaviour
    {
        private const float SPAWN_POSITION_OFFSET = 0.25f;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private float2 spawnerSize;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public int Team { get; private set; }
        //public Dictionary<ulong, Bounds> PlayerToSpawnBoundsMap { get; private set; }
        public Transform SpawnerTransform { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Getters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public Vector3 LocalPosition   => SpawnerTransform.localPosition;
        public Vector3 LocalLeft       => SpawnerTransform.localRotation * Vector3.left;
        public Vector3 LocalRight      => SpawnerTransform.localRotation * Vector3.right;
        public Vector3 LocalBack       => SpawnerTransform.localRotation * Vector3.back;
        public Vector3 LocalForward    => SpawnerTransform.localRotation * Vector3.forward;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void Awake()
        {
            SpawnerTransform = transform;
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void Initialize(float2 size, Material material, int team, List<ulong> playerIds)
        {
            spawnerSize = size;
            Mesh terrainMesh = MeshUtils.CreatePlaneMesh(size);
            meshFilter.sharedMesh = terrainMesh;
            meshRenderer.material = material;
            //AssignTeamAndPlayerSpawnAreas(team, playerIds);
        }
/*
        public void AssignTeamAndPlayerSpawnAreas(int team, List<ulong> playerIds)
        {
            Team = team;
            PlayerToSpawnBoundsMap = new Dictionary<ulong, Bounds>(playerIds.Count);
            NativeArray<Bounds> subSpawnersBound = SplitSpawnerByPlayers(playerIds.Count);
            for (int i = 0; i < playerIds.Count; i++)
            {
                PlayerToSpawnBoundsMap.Add(playerIds[i], subSpawnersBound[i]);
            }
        }

        private NativeArray<Bounds> SplitSpawnerByPlayers(int numPlayers)
        {
            NativeArray<Bounds> bounds = new (max(1, numPlayers), Temp, UninitializedMemory);
            float3 leftSide = Position + Left * (spawnerSize.x / 2f);
            
            float subSpawnerWidth = spawnerSize.x / bounds.Length;
            float halfSubSpawnerWidth = subSpawnerWidth / 2f;
            
            float3 subSpawnerSize = new (subSpawnerWidth, 1, spawnerSize.y);
            for (int i = 0; i < bounds.Length; i++)
            {
                float3 start = leftSide + (i * subSpawnerWidth) * Right;
                float3 centerSubSpawner = start + Right * halfSubSpawnerWidth;
                bounds[i] = new Bounds(centerSubSpawner, subSpawnerSize);
            }
            return bounds;
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Informations ◈◈◈◈◈◈                                                                                     ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public float3 GetPlayerFirstSpawnPosition(ulong playerId)
        {
            Bounds bound = PlayerToSpawnBoundsMap[playerId];
            float3 extents = (float3)bound.extents - SPAWN_POSITION_OFFSET;
            return (float3)bound.center + (Forward * extents.z) + (Left * extents.x);
        }
        */
    }
}
