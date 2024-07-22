/*
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static UnityEngine.Mathf;
using static UnityEngine.Quaternion;

using float2 = Unity.Mathematics.float2;
using float3 = Unity.Mathematics.float3;

using Kaizerwald.Pattern;
using Kaizerwald.Utilities.Core;
using UnityEngine.UIElements;
using static Kaizerwald.Utilities.Core.KzwMath;
using quaternion = Unity.Mathematics.quaternion;

namespace Kaizerwald.TerrainBuilder
{
    [ExecuteAfter(typeof(KaizerwaldGameManager), OrderIncrease = 1)]
    public class SpawnerManager2 : SingletonBehaviour<SpawnerManager2>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ Const/ReadOnly ◆◆◆◆◆◆                                            ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private enum ECardinal : int
        {
            North  = 0,
            South  = 1,
            East   = 2,
            West   = 3
        }
        
        private const int SPAWNER_DEPTH_SIZE = 16;
        private const int BORDER_OFFSET = 16;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [SerializeField] private GameObject SpawnerPrefab;
        [SerializeField] private Material SpawnerMaterial;
        [SerializeField] private TerrainSettings TerrainSettings;
        
        [SerializeField] private List<SpawnerComponent> Spawners;
        
        private Dictionary<int, SpawnerComponent> teamIdToSpawner;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public Transform TerrainTransform { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private int NumCardinals = 5;
        private Vector3 GetDirection(ECardinal direction) => direction switch
        {
            ECardinal.North  => Vector3.forward,
            ECardinal.South  => Vector3.back,
            ECardinal.East   => Vector3.right,
            ECardinal.West   => Vector3.left,
            _ => Vector3.zero,
        };
        
        public float3 TerrainPosition => TerrainTransform.position;
        public float2 SizeXY => TerrainSettings.SizeXY;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected override void OnAwake()
        {
            base.OnAwake();
            Initialize();
            //CreateAndInitializeSpawners();
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected override void Initialize()
        {
            TerrainTransform = transform;
            TerrainSettings = GetComponent<TerrainSettings>();
        }

        // =============================================================================================================
        // FIND A CALLER!!!!
        // =============================================================================================================

        public void CreateAndInitializeSpawners()
        {
            CreateAndInitializeSpawners(KaizerwaldGameManager.Instance.TeamIdToPlayerIdList);
        }

        public void CreateAndInitializeSpawners(Dictionary<int, List<ulong>> teamIdToPlayerIdListMap)
        {
            Spawners = new List<SpawnerComponent>(NumCardinals);
            teamIdToSpawner = new Dictionary<int, SpawnerComponent>(NumCardinals);
            int spawnerIndex = 0;
            foreach ((int teamId, List<ulong> playersId) in teamIdToPlayerIdListMap)
            {
                SpawnerComponent spawner = Instantiate(SpawnerPrefab, TerrainTransform).GetComponent<SpawnerComponent>();
                Spawners.Add(spawner);
                teamIdToSpawner.Add(teamId,spawner);
                
                float2 spawnerSize = float2(SizeXY.x - BORDER_OFFSET, SPAWNER_DEPTH_SIZE);
                spawner.Initialize(spawnerSize, SpawnerMaterial, teamId, playersId);
                spawner.transform.forward = -GetDirection((ECardinal)spawnerIndex);
                spawner.transform.position = GetSpawnerPosition((ECardinal)spawnerIndex);
                spawnerIndex++;
            }
        }
        
        private float3 GetSpawnerPosition(ECardinal direction)
        {
            float2 halfSizeXY = SizeXY / 2f;
            float3 dirOffset = GetDirection(direction);
            float3 offset = dirOffset * (dirOffset.x.IsZero() ? halfSizeXY.y : halfSizeXY.x);
            offset -= dirOffset * (SPAWNER_DEPTH_SIZE / 2f + BORDER_OFFSET);
            offset += up() * 0.01f;
            return offset;
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Spawner ◈◈◈◈◈◈                                                                                          ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public SpawnerComponent GetTeamSpawner(int teamId)
        {
            return teamIdToSpawner.GetValueOrDefault(teamId);
        }

        public bool TryGetTeamSpawner(int teamId, out SpawnerComponent teamSpawner)
        {
            return teamIdToSpawner.TryGetValue(teamId, out teamSpawner);
        }
        
        public float3 GetPlayerFirstSpawnPosition(int teamId, ulong playerId)
        {
            return teamIdToSpawner.TryGetValue(teamId, out SpawnerComponent spawner) ? spawner.GetPlayerFirstSpawnPosition(playerId) : float3.zero;
        }
        
        public float3 GetPlayerFirstSpawnPosition(int spawnerIndex)
        {
            return Spawners[spawnerIndex].SpawnerTransform.position;
            //return teamIdToSpawner.TryGetValue(teamId, out SpawnerComponent spawner) ? spawner.GetPlayerFirstSpawnPosition(playerId) : float3.zero;
        }
        
        //public Vector3 GetPlayerFirstSpawnPosition(int spawnIndex)
        //{
        //    Transform spawnerTransform = GetSpawnerTransform(spawnIndex);
        //    if(spawnerTransform == null) return Vector3.zero;
        //    
        //    float spawnHorizontalSize = (SizeXY.x / 2f) - BORDER_OFFSET;
        //    Vector3 left = -spawnerTransform.right;
        //    Vector3 firstSpawnPoint = spawnerTransform.position + left * spawnHorizontalSize;
        //    return Vector3.Scale(firstSpawnPoint, new Vector3(1f,0,1f));
        //}
    }
}

*/