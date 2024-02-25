using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

using Kaizerwald.Utilities;
using Kaizerwald.FormationModule;

namespace Kaizerwald
{
    public interface IPlayersRegimentsCommand
    {
        public Dictionary<ulong, List<RegimentSpawner>> PlayerKeyPairSpawners { get; }
        public Dictionary<int, Dictionary<ulong, List<RegimentSpawner>>> TeamKeyPairPlayerSpawners { get; }
    }
    
    public sealed class RegimentFactory : Singleton<RegimentFactory>
    {
        private const float SPACE_BETWEEN_REGIMENT = 1f;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [SerializeField] private RegimentPrefabsList RegimentPrefabsList;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private Dictionary<int, float> TeamKeyPairOffset = new Dictionary<int, float>(2);
        
        private LayerMask TerrainLayerMask => KaizerwaldGameManager.Instance.TerrainLayerMask;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected override void OnAwake()
        {
            base.OnAwake();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private float GetOffset(RegimentSpawner spawner, int index, bool sameRegiment = false)
        {
            if (index == 0) return 0;
            FormationData formation = spawner.RegimentType.GetFormationData();

            int defaultRow = formation.MinRow;
            //int defaultRow = min(formation.MinMaxRow[0], formation.MinMaxRow[1] / 2);
            // we add an extra space if not same regiment (debug purpose)
            return defaultRow * formation.DistanceUnitToUnitX / (sameRegiment ? 1f : 2f);
        }

        public List<Regiment> RequestRegiments(IPlayersRegimentsCommand playersRegimentsCommand)
        {
            return RegimentPrefabsList == null ? null : CreateRegiments(playersRegimentsCommand.TeamKeyPairPlayerSpawners);
        }
        
        private List<Regiment> CreateRegiments(Dictionary<int, Dictionary<ulong, List<RegimentSpawner>>> spawnerByTeam)
        {
            //TODO: !!not accurate since a commands may have a number property > 1
            List<Regiment> regimentsCreated = new (spawnerByTeam.Values.Sum(pair => pair.Values.Count));
            int teamIndex = 0;// because may enter team like (0,2,6).. need to fixe that
            foreach ((int teamId, Dictionary<ulong, List<RegimentSpawner>> teamSpawners) in spawnerByTeam)
            {
                TeamKeyPairOffset[teamId] = 0;
                
                Vector3 startPosition = TerrainManager.Instance.GetPlayerFirstSpawnPosition(teamIndex);
                Transform spawnerTransform = TerrainManager.Instance.GetSpawnerTransform(teamIndex);
                
                foreach ((ulong playerId, List<RegimentSpawner> playerSpawners) in teamSpawners)
                {
                    for (int spawnerIndex = 0; spawnerIndex < playerSpawners.Count; spawnerIndex++)
                    {
                        (RegimentSpawner current, RegimentSpawner previous) = (playerSpawners[spawnerIndex], playerSpawners[max(0,spawnerIndex-1)]);
                        TeamKeyPairOffset[teamId] += GetOffset(current, spawnerIndex) + GetOffset(previous, spawnerIndex);
                        
                        GameObject regimentPrefab = RegimentPrefabsList.GetPrefabFromRegimentType(current.RegimentType);
                        for (int cloneIndex = 0; cloneIndex < current.Number; cloneIndex++)
                        {
                            TeamKeyPairOffset[teamId] += GetOffset(current, cloneIndex, true) + SPACE_BETWEEN_REGIMENT;
                            Vector3 position = startPosition + TeamKeyPairOffset[teamId] * spawnerTransform.right;

                            Regiment regiment = Instantiate(regimentPrefab, position, spawnerTransform.rotation).GetOrAddComponent<Regiment>();
                            regimentsCreated.Add(regiment.InitializeProperties(playerId, teamId, TerrainLayerMask));
                        }
                    }
                }
                teamIndex++;
            }
            return regimentsCreated;
        }
    }
    
}