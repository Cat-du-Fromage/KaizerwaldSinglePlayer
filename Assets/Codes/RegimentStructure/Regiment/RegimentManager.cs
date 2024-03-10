using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.SceneManagement;
using UnityEngine;

using Kaizerwald.Utilities;
using Kaizerwald.FormationModule;
using Kaizerwald.TerrainBuilder;

namespace Kaizerwald
{
    [ExecuteAfter(typeof(SimpleTerrain), OrderIncrease = 1)]
    public class RegimentManager : Singleton<RegimentManager>
    {
        public const float RegimentFieldOfView = 60f;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Containers ◈◈◈◈◈◈                                                                                       ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public List<Regiment> Regiments { get; private set; } = new ();
        
        // Allow to retrieve regiment By it's Instance ID
        public Dictionary<int, Regiment> RegimentsByID { get; private set; } = new ();
        
        // Allow to retrieve regiments of a player
        public Dictionary<ulong, List<Regiment>> RegimentsByPlayerID { get; private set; } = new ();
        
        // Allow to retrieve regiments of a team
        public Dictionary<int, List<Regiment>> RegimentsByTeamID { get; private set; } = new ();
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Events ◈◈◈◈◈◈                                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public event Action<Regiment> OnNewRegiment;
        public event Action<GameObject> OnDeadRegiment;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Awake | Start ◈◈◈◈◈◈                                                                                    ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        /*
        private async void Start()
        {
            // Allow to other managers to subscribe to "OnNewRegiment" Before they are created!
            // Can't use Awake because it may force to create a New RegimentManager because Order is not controlled!
            await Awaitable.NextFrameAsync();
            List<Regiment> regiments = RegimentFactory.Instance.RequestRegiments(SpawnCommandManager.Instance);
            regiments.ForEach(RegisterRegiment);
            
            HighlightRegimentManager.Instance.OnPlayerOrders += OnPlayerOrdersReceived;
        }
        */
        private void Start()
        {
            List<Regiment> regiments = RegimentFactory.Instance.RequestRegiments(SpawnCommandManager.Instance);
            regiments.ForEach(RegisterRegiment);
            
            HighlightRegimentManager.Instance.OnPlayerOrders += OnPlayerOrdersReceived;
            HighlightAbilityController.Instance.OnAbilityTriggered += OnPlayerInput;
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Update | Late Update ◈◈◈◈◈◈                                                                             ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public void FixedUpdate()
        {
            for (int i = 0; i < Regiments.Count; i++)
            {
                Regiments[i].OnFixedUpdate();
            }
        }

        public void Update()
        {
            for (int i = 0; i < Regiments.Count; i++)
            {
                Regiments[i].OnUpdate();
            }
        }

        public void LateUpdate()
        {
            CleanupEmptyRegiments();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public bool RegimentExist(int regimentID) => RegimentsByID.ContainsKey(regimentID);
        public bool RegimentExist(Regiment regiment)
        {
            return regiment != null && RegimentsByID.ContainsKey(regiment.RegimentID);
        }

        public bool TryGetRegiment(int regimentID, out Regiment regiment) => RegimentsByID.TryGetValue(regimentID, out regiment);

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Num Units ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public int GetEnemiesTeamNumUnits(int friendlyTeamID)
        {
            int numUnits = 0;
            foreach ((int key, List<Regiment> regiments) in RegimentsByTeamID)
            {
                bool isEnemyTeam = key != friendlyTeamID;
                numUnits += isEnemyTeam ? regiments.Sum(pair => pair.Count) : 0;
            }
            return numUnits;
        }
        
        public int GetTeamNumUnits(int teamId)
        {
            return RegimentsByTeamID.TryGetValue(teamId, out List<Regiment> regiments) ? regiments.Sum(reg => reg.Count) : 0;
        }

        private void CleanupEmptyRegiments()
        {
            if (Regiments.Count == 0) return;
            for (int i = Regiments.Count - 1; i > -1; i--)
            {
                Regiment regiment = Regiments[i];
                if(regiment.Count > 0) continue;
                UnRegisterRegiment(regiment);
            }
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Add | Remove ◈◈◈◈◈◈                                                                                     ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void RegisterRegiment(Regiment regiment)
        {
            Regiments.Add(regiment);
            RegimentsByID.TryAdd(regiment.RegimentID, regiment);
            RegimentsByPlayerID.AddSafe(regiment.OwnerPlayerID, regiment);
            RegimentsByTeamID.AddSafe(regiment.TeamID, regiment);
            OnNewRegiment?.Invoke(regiment);
        }
        
        public void UnRegisterRegiment(Regiment regiment)
        {
            OnDeadRegiment?.Invoke(regiment.gameObject);
            Regiments.Remove(regiment);
            RegimentsByID.Remove(regiment.RegimentID);
            RegimentsByPlayerID[regiment.OwnerPlayerID].Remove(regiment);
            RegimentsByTeamID[regiment.TeamID].Remove(regiment);
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ ORDER METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void OnPlayerOrdersReceived(PlayerOrderData[] orders)
        {
            foreach (PlayerOrderData order in orders)
            {
                //if (!RegimentExist(order.RegimentID)) continue;
                if (!RegimentsByID.TryGetValue(order.RegimentID, out Regiment regiment)) continue;
                regiment.OnOrderReceived(order);
            }
            
            // if OrderType == Move => Mettre a 0 les variables lié à une poursuite
        }
        
        private void OnPlayerInput(AbilityTrigger[] abilities)
        {
            foreach (AbilityTrigger ability in abilities)
            {
                if (!RegimentsByID.TryGetValue(ability.RegimentID, out Regiment regiment)) continue;
                regiment.OnAbilityTriggered(ability.AbilityType);
            }
        }
    }
}
