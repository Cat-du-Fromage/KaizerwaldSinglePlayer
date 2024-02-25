using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.SceneManagement;
using UnityEngine;

using Kaizerwald.Utilities;
using Kaizerwald.FormationModule;

namespace Kaizerwald
{
    public class RegimentManager : Singleton<RegimentManager>, IGameSystem
    {
        public int ExecutionOrderWeight => 1;
        
        public const float RegimentFieldOfView = 60f;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //public bool Initialized { get; private set; }
        //public event Action OnManagerInitialized;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //[field:SerializeField] public LayerMask TerrainLayerMask { get; private set; }
        //[field:SerializeField] public LayerMask UnitLayerMask { get; private set; }
        
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
        public void OnStart()
        {
            //Debug.Log($"RegimentManager PriorityOrder = {PriorityOrder}");
            List<Regiment> regiments = RegimentFactory.Instance.RequestRegiments(SpawnCommandManager.Instance);
            regiments.ForEach(RegisterRegiment);
            HighlightRegimentManager.Instance.OnPlayerOrders += OnPlayerOrdersReceived;
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Update | Late Update ◈◈◈◈◈◈                                                                             ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public void OnFixedUpdate()
        {
            for (int i = 0; i < Regiments.Count; i++)
            {
                Regiments[i].OnFixedUpdate();
            }
        }

        public void OnUpdate()
        {
            for (int i = 0; i < Regiments.Count; i++)
            {
                Regiments[i].OnUpdate();
            }
        }

        public void OnLateUpdate()
        {
            CleanupEmptyRegiments();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public bool RegimentExist(int regimentID) => RegimentsByID.ContainsKey(regimentID);
        public bool RegimentExist(Regiment regiment)
        {
            if (regiment == null) return false;
            return RegimentsByID.ContainsKey(regiment.RegimentID);
        }

        public bool TryGetRegiment(int regimentID, out Regiment regiment) => RegimentsByID.TryGetValue(regimentID, out regiment);
        public bool TryGetRegiment(Regiment regiment, out Regiment enemyRegiment)
        {
            if (regiment != null) return RegimentsByID.TryGetValue(regiment.RegimentID, out enemyRegiment);
            enemyRegiment = null;
            return false;
        }

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
            //HashSet<Regiment> toDestroy = new HashSet<Regiment>(1);
            for (int i = Regiments.Count - 1; i > -1; i--)
            {
                Regiment regiment = Regiments[i];
                if(regiment.Count > 0) continue;
                //Debug.Log($"RegimentManager.CleanupEmptyRegiments : regiment.Count = {regiment.Count}, formation num alive : {regiment.CurrentFormation.NumUnitsAlive}");
                //toDestroy.Add(regiment);
                UnRegisterRegiment(regiment);
                //Destroy(regiment.gameObject);
            }
            
            //if (toDestroy.Count == 0) return;
            //foreach (Regiment regimentToDestroy in toDestroy) Destroy(regimentToDestroy.gameObject);
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
                if (!RegimentExist(order.RegimentID)) continue;
                RegimentsByID[order.RegimentID].OnOrderReceived(order);
            }
        }
    }
}
