using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

using Kaizerwald.Utilities;
using Kaizerwald.FormationModule;

namespace Kaizerwald
{
    //FAIRE de régiment manager une partie intégrante de l'outil "HighlightRegimentManager"
    public sealed class HighlightRegimentManager : Singleton<HighlightRegimentManager>, IOwnershipInformation, IGameSystem
    {
        public int PriorityOrder => 2;
        public void OnStartSystem() { Debug.Log($"HighlightRegimentManager PriorityOrder = {PriorityOrder}"); }
        
        // IOwnershipInformation
        [field:SerializeField] public ulong OwnerPlayerID { get; private set; }
        [field:SerializeField] public int TeamID { get; private set; }
        
        [field:Header("Layer Masks")]
        [field:SerializeField] public LayerMask TerrainLayerMask { get; private set; }
        [field:SerializeField] public LayerMask UnitLayerMask { get; private set; }
        
        [field:Header("Default Prefabs")]
        [field:SerializeField] public GameObject PreselectionDefaultPrefab { get; private set; }
        [field:SerializeField] public GameObject SelectionDefaultPrefab { get; private set; }
        [field:SerializeField] public GameObject PlacementDefaultPrefab { get; private set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private List<HighlightController> Controllers = new (2);
        private List<HighlightSystem> highlightSystems = new (2);
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public PlayerControls HighlightControls { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Highlights ◈◈◈◈◈◈                                                                                       ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public SelectionSystem Selection { get; private set; }
        public PlacementSystem Placement { get; private set; }
        
        public List<HighlightRegiment> PreselectedRegiments => Selection.PreselectionRegister.ActiveHighlights;
        public List<HighlightRegiment> SelectedRegiments => Selection.SelectionRegister.ActiveHighlights;

        public SelectionInfos SelectionInfos => Selection.SelectionInfos;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Containers ◈◈◈◈◈◈                                                                                       ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        [field:SerializeField] public List<HighlightRegiment> Regiments { get; private set; } = new ();
        
        //Allow to retrieve regiment By it's Instance ID
        public Dictionary<int, HighlightRegiment> RegimentsByID { get; private set; } = new ();
        
        //Allow to retrieve regiments of a player
        public Dictionary<ulong, List<HighlightRegiment>> RegimentsByPlayerID { get; private set; } = new ();
        
        //Allow to retrieve regiments of a team
        public Dictionary<int, List<HighlightRegiment>> RegimentsByTeamID { get; private set; } = new ();
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Events ◈◈◈◈◈◈                                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public event Action<PlayerOrderData[]> OnPlayerOrders;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Awake | Start ◈◈◈◈◈◈                                                                                    ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        protected override void OnAwake()
        {
            base.OnAwake();
            HighlightControls = new PlayerControls();
            Selection = new SelectionSystem(this);
            Placement = new PlacementSystem(this);
            highlightSystems = new List<HighlightSystem>() { Selection, Placement };
            Controllers = new List<HighlightController>() { Selection.Controller, Placement.Controller };
        }

        private void Start()
        {
            RegimentManager.Instance.OnNewRegiment += InitAndRegisterRegiment<Regiment, Unit>;
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Update | Late Update ◈◈◈◈◈◈                                                                             ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private void Update()
        {
            Controllers.ForEach(controller => controller.OnUpdate());
        }

        private void FixedUpdate()
        {
            Controllers.ForEach(controller => controller.OnFixedUpdate());
            foreach (HighlightRegiment highlightRegiment in Regiments)
            {
                highlightRegiment.OnFixedUpdate();
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < Regiments.Count; i++)
            {
                HighlightRegiment highlightRegiment = Regiments[i];
                int highlightCount = highlightRegiment.Count;
                int countAt = Selection.PreselectionRegister.CountAt(highlightRegiment.RegimentID);
                bool needResize = highlightCount == countAt;
                if (needResize) continue;
                ResizeHighlightsRegisters(highlightRegiment);
            }

            CleanupEmptyRegiments();
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Enable | Disable ◈◈◈◈◈◈                                                                                 ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        public void OnEnable()
        {
            Controllers?.ForEach(controller => controller.OnEnable());
        }

        public void OnDisable()
        {
            Controllers?.ForEach(controller => controller.OnDisable());
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void SetPlayerID(ulong playerID) => OwnerPlayerID = playerID;
        public void SetTeamID(int teamID) => TeamID = teamID;
        
        public void SetPlayerInfos(ulong playerID, int teamID)
        {
            SetPlayerID(playerID);
            SetTeamID(teamID);
        }

        public bool RegimentExist(int regimentID)
        {
            return RegimentsByID.ContainsKey(regimentID);
        }
        
        private void CleanupEmptyRegiments()
        {
            if (Regiments.Count == 0) return;
            for (int i = Regiments.Count - 1; i > -1; i--)
            {
                HighlightRegiment regiment = Regiments[i];
                if (regiment.CurrentFormation.NumUnitsAlive > 0) continue;
                UnRegisterRegiment(regiment);
                Destroy(regiment);
            }
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Request Registration ◈◈◈◈◈◈                                                                             ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        /*
        public void RequestHighlightAttachment<T1,T2>(ulong ownerID, int teamID, T1 formationMatrix)
        where T1 : OrderedFormationBehaviour<T2>
        where T2 : Component, IFormationElement
        {
            List<GameObject> unitsObject = formationMatrix.Elements.Select(unit => unit.gameObject).ToList();
            InitAndRegisterRegiment(ownerID, teamID, formationMatrix.gameObject, unitsObject, formationMatrix.CurrentFormation);
        }
        */
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Initialize AND Register ◈◈◈◈◈◈                                                                          ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        //TODO: Allow Identifier Override so we can use NetworkHashId for multiplayer games, instead of GetInstanceID() which is different for each Client!
        /*
        public void InitAndRegisterRegiment(ulong ownerID, int teamID, GameObject regimentGo, List<GameObject> units, int2 minMaxRow, float2 unitSize, float spaceBetweenUnit, float3 direction)
        {
            HighlightRegiment newHighlightRegiment = regimentGo.AddComponent<HighlightRegiment>();
            newHighlightRegiment.InitializeHighlight(ownerID, teamID, units, minMaxRow, unitSize, spaceBetweenUnit, direction);
            RegisterRegiment(newHighlightRegiment, units);
        }

        public void InitAndRegisterRegiment(ulong ownerID, int teamID, GameObject regimentGo, List<GameObject> units, Formation formation)
        {
            InitAndRegisterRegiment(ownerID, teamID, regimentGo, units, formation.MinMaxRow, formation.UnitSize, formation.SpaceBetweenUnits, formation.DirectionForward);
        }
        */
        public void InitAndRegisterRegiment<TRegiment, TUnit>(TRegiment regiment)
        where TRegiment : BaseFormationBehaviour<TUnit>, IOwnershipInformation
        where TUnit : Component, IFormationElement
        {
            ulong ownerID = regiment.OwnerPlayerID;
            int teamID = regiment.TeamID;
            List<GameObject> unitsObject = regiment.Elements.Select(unit => unit.gameObject).ToList();
            Formation formation = regiment.CurrentFormation;
            
            HighlightRegiment newHighlightRegiment = regiment.gameObject.AddComponent<HighlightRegiment>();
            newHighlightRegiment.InitializeHighlight(ownerID, teamID, unitsObject, formation);
            
            RegisterRegiment(newHighlightRegiment, unitsObject);
        }
    
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Regiment Update Event ◇◇◇◇◇◇                                                                       │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void BaseRegister(HighlightRegiment regiment)
        {
            Regiments.Add(regiment);
            RegimentsByID.TryAdd(regiment.RegimentID, regiment);
            RegimentsByPlayerID.AddSafe(regiment.OwnerID, regiment);
            RegimentsByTeamID.AddSafe(regiment.TeamID, regiment);
        }
        
        public void RegisterRegiment<T>(HighlightRegiment regiment, List<T> units) 
        where T : MonoBehaviour
        {
            BaseRegister(regiment);
            Selection.AddRegiment(regiment,units);
            Placement.AddRegiment(regiment,units);
        }
        
        public void RegisterRegiment(HighlightRegiment regiment, List<GameObject> units)
        {
            BaseRegister(regiment);
            Selection.AddRegiment(regiment,units);
            Placement.AddRegiment(regiment,units);
        }
        
        public void UnRegisterRegiment(HighlightRegiment regiment)
        {
            Selection.RemoveRegiment(regiment);
            Placement.RemoveRegiment(regiment);
            Regiments.Remove(regiment);
            RegimentsByID.Remove(regiment.RegimentID);
            RegimentsByPlayerID[regiment.OwnerID].Remove(regiment);
            RegimentsByTeamID[regiment.TeamID].Remove(regiment);
        }
        
        public void UnRegisterRegiment(GameObject regimentGameObject)
        {
            if (!regimentGameObject.TryGetComponent(out HighlightRegiment regiment)) return;
            UnRegisterRegiment(regiment);
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ OUTSIDES UPDATES / Resize Request ◈◈◈◈◈◈                                                            ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void ResizeHighlightsRegisters(HighlightRegiment regiment)
        {
            foreach (HighlightSystem highlightSystem in highlightSystems)
            {
                highlightSystem.ResizeRegister(regiment);
            }
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ PLAYER ORDER ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        public void OnPlayerOrder(PlayerOrderData[] orders)
        {
            OnPlayerOrders?.Invoke(orders);
        }
        
    }
}