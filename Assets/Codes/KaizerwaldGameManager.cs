using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Kaizerwald.Pattern;
using Kaizerwald.Utilities.Core;
using Unity.Mathematics;

namespace Kaizerwald
{
    [ExecutionOrder(0)]
    public class KaizerwaldGameManager : Singleton<KaizerwaldGameManager>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [SerializeField] private bool IsInitialized;
        public Dictionary<short, List<PlayerInfos>> TeamToPlayersInfo { get; private set; } = new() { { 0, new List<PlayerInfos> {default} } };
        public Dictionary<int, List<ulong>> TeamIdToPlayerIdList { get; private set; } = new() { { 0, new List<ulong> {default} } };
        
        public Dictionary<ulong, PlayerInfos> PlayerIDToPlayersInfo { get; private set; } = new(){ { 0, default } };

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [field:SerializeField] public uint MaxPlayer { get; private set; }

        [field:SerializeField] public LayerMask TerrainLayerMask { get; private set; }
        [field:SerializeField] public LayerMask UnitLayerMask { get; private set; }

        [field: SerializeField] public List<PlayerInfos> PlayersInfo { get; private set; } = new() { default };

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public int TerrainLayerIndex => TerrainLayerMask.GetLayerIndex();
        public int UnitLayerIndex => UnitLayerMask.GetLayerIndex();
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected override void OnAwake()
        {
            base.OnAwake();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!IsInitialized)
            {
                Initialize();
                IsInitialized = true;
            }
        }
#endif
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void Initialize()
        {
            PlayersInfo = new List<PlayerInfos>(){ default };
            TeamToPlayersInfo = new () { { PlayersInfo[0].TeamID, PlayersInfo } };
            PlayerIDToPlayersInfo = new () { { PlayersInfo[0].OwnerPlayerID, PlayersInfo[0] } };
        }
        
        //BUG: For now we dont check for duplicate PLAYER ID! player id must be UNIQUE!
        public void Initialize(List<PlayerInfos> players)
        {
            PlayersInfo = players.Count > 0 ? players : new List<PlayerInfos>(){ default };
            
            TeamToPlayersInfo = new Dictionary<short, List<PlayerInfos>>(players.Count);
            TeamIdToPlayerIdList = new(players.Count);
            
            PlayerIDToPlayersInfo = new Dictionary<ulong, PlayerInfos>(players.Count);
            foreach (PlayerInfos playerInfos in players)
            {
                TeamToPlayersInfo.AddSafe(playerInfos.TeamID, playerInfos);
                TeamIdToPlayerIdList.AddSafe(playerInfos.TeamID, playerInfos.OwnerPlayerID);
                
                PlayerIDToPlayersInfo.TryAdd(playerInfos.OwnerPlayerID, playerInfos);
            }
        }
        /*
        public Dictionary<int, List<ulong>> TeamIdToPlayerIdList()
        {
            Dictionary<int, List<ulong>> teamIdToPlayerIdListMap = new(KaizerwaldGameManager.Instance.TeamToPlayersInfo.Count);
            foreach ((short teamId, List<PlayerInfos> players) in KaizerwaldGameManager.Instance.TeamToPlayersInfo)
            {
                teamIdToPlayerIdListMap.Add(teamId, new List<ulong>(players.Count));
                foreach (PlayerInfos playerInfo in players)
                {
                    teamIdToPlayerIdListMap[teamId].Add(playerInfo.OwnerPlayerID);
                }
            }
            return teamIdToPlayerIdListMap;
        }
        */
    }
}
