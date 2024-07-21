using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Kaizerwald.Pattern;
using Kaizerwald.Utilities.Core;

namespace Kaizerwald
{
    public class SpawnCommandManager : Singleton<SpawnCommandManager>, IPlayersRegimentsCommand
    {
        // Use for convieniance/debug purpose
        //[field: SerializeField] public RegimentSpawner[] CreationOrders { get; private set; }
        [field: SerializeField] public List<RegimentSpawner> CreationOrders { get; private set; } = new ();
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public Dictionary<ulong, List<RegimentSpawner>> PlayerKeyPairSpawners { get; private set; } = new();
        public Dictionary<int, Dictionary<ulong, List<RegimentSpawner>>> TeamKeyPairPlayerSpawners { get; private set; } = new();
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        protected override void OnAwake()
        {
            base.OnAwake();
            TeamKeyPairPlayerSpawners = DivideSpawnOrdersByTeam(CreationOrders);
            PlayerKeyPairSpawners = GetSpawnersPerPlayerID(TeamKeyPairPlayerSpawners);
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void AddRegimentSpawner(ulong playerId, int teamId, RegimentType regimentType)
        {
            RegimentSpawner spawner = new RegimentSpawner
            {
                PlayerID = playerId,
                TeamID = teamId,
                Number = 1,
                RegimentType = regimentType
            };
            CreationOrders.Add(spawner);
        }

        public void Initialize()
        {
            TeamKeyPairPlayerSpawners = DivideSpawnOrdersByTeam(CreationOrders);
            PlayerKeyPairSpawners = GetSpawnersPerPlayerID(TeamKeyPairPlayerSpawners);
        }

        private Dictionary<int, Dictionary<ulong, List<RegimentSpawner>>> DivideSpawnOrdersByTeam(IEnumerable<RegimentSpawner> orders)
        {
            Dictionary<int, Dictionary<ulong, List<RegimentSpawner>>> spawnerByTeam = new (4);
            foreach (RegimentSpawner order in orders)
            {
                if (!spawnerByTeam.TryGetValue(order.TeamID, out Dictionary<ulong, List<RegimentSpawner>> teamSpawnersDictionary))
                {
                    teamSpawnersDictionary = new Dictionary<ulong, List<RegimentSpawner>>();
                    spawnerByTeam.Add(order.TeamID, teamSpawnersDictionary);
                }
                if (!teamSpawnersDictionary.TryGetValue(order.PlayerID, out List<RegimentSpawner> playerSpawnerList))
                {
                    playerSpawnerList = new List<RegimentSpawner>();
                    teamSpawnersDictionary.Add(order.PlayerID, playerSpawnerList);
                }
                playerSpawnerList.Add(order);
            }
            return spawnerByTeam;
        }

        private Dictionary<ulong, List<RegimentSpawner>> GetSpawnersPerPlayerID(Dictionary<int, Dictionary<ulong, List<RegimentSpawner>>> teamKeyPairPlayerSpawners)
        {
            int numValues = teamKeyPairPlayerSpawners.Values.Count;
            Dictionary<ulong, List<RegimentSpawner>> playerKeyPairSpawners = new (numValues);
            foreach (Dictionary<ulong, List<RegimentSpawner>> teamPlayerSpawners in teamKeyPairPlayerSpawners.Values)
            {
                foreach ((ulong playerId, List<RegimentSpawner> playerSpawners) in teamPlayerSpawners)
                {
                    playerKeyPairSpawners.TryAdd(playerId, playerSpawners);
                }
            }
            return playerKeyPairSpawners;
        }
    }
}
