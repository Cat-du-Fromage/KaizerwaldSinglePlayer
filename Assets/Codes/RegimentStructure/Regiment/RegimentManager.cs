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
    public class RegimentManager : Singleton<RegimentManager>, IManagerInitialization, IGameSystem
    {
        public int PriorityOrder => 3;
        public void OnStartSystem() { Debug.Log($"RegimentManager PriorityOrder = {PriorityOrder}"); }
        
        
        public const float RegimentFieldOfView = 60f;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //[SerializeField] 

        public bool Initialized { get; private set; }
        public event Action OnManagerInitialized;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [field:SerializeField] public LayerMask TerrainLayerMask { get; private set; }
        [field:SerializeField] public LayerMask UnitLayerMask { get; private set; }
        
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
        public event Action<Regiment> OnDeadRegiment;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Awake | Start ◈◈◈◈◈◈                                                                                    ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private async void Start()
        {
            // Allow to other managers to subscribe to "OnNewRegiment" Before they are created!
            // Can't use Awake because it may force to create a New RegimentManager because Order is not controlled!
            await Awaitable.NextFrameAsync();
            List<Regiment> regiments = RegimentFactory.Instance.RequestRegiments(SpawnCommandManager.Instance);
            regiments.ForEach(RegisterRegiment);
            
            HighlightRegimentManager.Instance.OnPlayerOrders += OnPlayerOrdersReceived;
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Update | Late Update ◈◈◈◈◈◈                                                                             ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void FixedUpdate()
        {
            //Regiments.ForEach(regiment => regiment.OnFixedUpdate());
            for (int i = 0; i < Regiments.Count; i++)
            {
                Regiments[i].OnFixedUpdate();
            }
        }

        private void Update()
        {
            //Regiments.ForEach(regiment => regiment.OnUpdate());
            for (int i = 0; i < Regiments.Count; i++)
            {
                Regiments[i].OnUpdate();
            }
        }

        private void LateUpdate()
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
            for (int i = Regiments.Count - 1; i > -1; i--)
            {
                Regiment regiment = Regiments[i];
                if(regiment.Count > 0) continue;
                UnRegisterRegiment(regiment);
                Destroy(regiment.gameObject);
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
            OnNewRegiment?.Invoke(regiment); //MAYBE USELESS
        }
        
        public void UnRegisterRegiment(Regiment regiment)
        {
            OnDeadRegiment?.Invoke(regiment); //MAYBE USELESS
            Regiments.Remove(regiment);
            RegimentsByID.Remove(regiment.RegimentID);
            RegimentsByPlayerID[regiment.OwnerPlayerID].Remove(regiment);
            RegimentsByTeamID[regiment.TeamID].Remove(regiment);
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Highlights ◈◈◈◈◈◈                                                                                       ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜ 
        /*
        private void HighlightsAttachments()
        {
            HighlightRegimentManager.Instance.SetPlayerInfos(0,0);
            foreach (Regiment regiment in Regiments)
            {
                AttachHighlight<OrderedFormationBehaviour<Unit>,Unit>(regiment.OwnerPlayerID, regiment.TeamID, regiment);
            }
            //Regiments.ForEach(regiment => AttachHighlight<RegimentFormationMatrix,Unit>(regiment.OwnerPlayerID, regiment.TeamID, regiment.RegimentFormationMatrix));
        }
        
        private void AttachHighlight<T1,T2>(ulong ownerId, int teamId, T1 formationMatrix)
        where T1 : OrderedFormationBehaviour<T2>
        where T2 : Component, IFormationElement
        {
            HighlightRegimentManager.Instance.RequestHighlightAttachment<T1,T2>(ownerId,teamId,formationMatrix);
        }
        */
        // CANT call it from event : No link to RegimentType + Units possible to HighlightManager
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ ORDER METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void OnPlayerOrdersReceived(PlayerOrderData[] orders)
        {
            foreach (PlayerOrderData order in orders)
            {
                RegimentsByID[order.RegimentID].OnOrderReceived(order);
            }
        }
    }
}
