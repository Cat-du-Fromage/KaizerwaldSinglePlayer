using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Kaizerwald.Utilities;
using Unity.Mathematics;

namespace Kaizerwald
{
    public class KaizerwaldGameManager : Singleton<KaizerwaldGameManager>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [SerializeField] private bool IsInitialized;
        private Dictionary<short, List<PlayerInfos>> TeamToPlayersInfo = new() { { 0, new List<PlayerInfos> {default} } };
        private Dictionary<ulong, PlayerInfos> PlayerIDToPlayersInfo = new(){ { 0, default } };

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
            PlayerIDToPlayersInfo = new Dictionary<ulong, PlayerInfos>(players.Count);
            foreach (PlayerInfos playerInfos in players)
            {
                TeamToPlayersInfo.AddSafe(playerInfos.TeamID, playerInfos);
                PlayerIDToPlayersInfo.TryAdd(playerInfos.OwnerPlayerID, playerInfos);
            }
        }
    }
}
