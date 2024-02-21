using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

using static UnityEngine.Physics;
using static Unity.Mathematics.math;
using static Unity.Mathematics.quaternion;
using static Unity.Collections.Allocator;

using Kaizerwald.FormationModule;
using Kaizerwald.Utilities;

namespace Kaizerwald
{
    public sealed class PlacementSystem : HighlightSystem
    {
        public enum EPlacementRegister : int
        {
            Static = 0,
            Dynamic = 1
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public SelectionInfos SelectionInfos => Manager.SelectionInfos;
        public List<HighlightRegiment> PreselectedRegiments => Manager.PreselectedRegiments;
        public List<HighlightRegiment> SelectedRegiments => Manager.SelectedRegiments;
        
        public HighlightRegister StaticPlacementRegister => Registers[(int)EPlacementRegister.Static];
        public HighlightRegister DynamicPlacementRegister => Registers[(int)EPlacementRegister.Dynamic];

        public PlacementController PlacementController => (PlacementController)Controller;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public PlacementSystem(HighlightRegimentManager manager) : base(manager)
        {
            InitializeController();
            InitializeRegisters();
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Orders Callback ◈◈◈◈◈◈                                                                                  ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        public void OnMoveOrderEvent(int registerIndexUsed, bool marchOrdered)
        {
            bool keepSameFormation = PlacementController.DynamicsTempWidth.Length == 0;
            PlayerOrderData[] moveOrders = new PlayerOrderData[SelectedRegiments.Count];
            for (int i = 0; i < PlacementController.SortedSelectedRegiments.Count; i++)
            {
                HighlightRegiment regiment = PlacementController.SortedSelectedRegiments[i];
                if (regiment == null) continue;
                int width = keepSameFormation ? regiment.CurrentFormation.Width : PlacementController.DynamicsTempWidth[i];
                int numUnitsAlive = regiment.CurrentFormation.NumUnitsAlive;
                width = width > numUnitsAlive ? numUnitsAlive : width;
                
                moveOrders[i] = PackOrder(registerIndexUsed, regiment, width, marchOrdered);
                regiment.SetDestination(moveOrders[i].LeaderDestination, moveOrders[i].TargetFormation);
            }
            Manager.OnPlayerOrder(moveOrders);
        }

        private PlayerOrderData PackOrder(int registerIndexUsed, HighlightRegiment regiment, int width, bool marchOrdered)
        {
            float3 firstUnit = Registers[registerIndexUsed][regiment.RegimentID][0].transform.position;
            float3 lastUnit = Registers[registerIndexUsed][regiment.RegimentID][width-1].transform.position;
            float3 direction = normalizesafe(cross(down(), lastUnit - firstUnit));
            FormationData formationDestination = new (regiment.CurrentFormation, width, direction);
            float3 leaderDestination = (firstUnit + lastUnit) / 2f;
            
            PlayerOrderData orderData = new PlayerOrderData
            {
                RegimentID = regiment.RegimentID,
                OrderType = EOrderType.Move,
                MovePace = marchOrdered ? EMovePace.March : EMovePace.Run,
                LeaderDestination = leaderDestination,
                TargetFormation = formationDestination,
                TargetEnemyID = -1
            };
            
            return orderData;
        }
        

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        protected override void InitializeController()
        {
            Controller = new PlacementController(this, Manager.HighlightControls, Manager.TerrainLayerMask);
        }

        protected override void InitializeRegisters()
        {
            Registers[0] = new HighlightRegister(this, Manager.PlacementDefaultPrefab);
            Registers[1] = new HighlightRegister(this, Manager.PlacementDefaultPrefab);
        }

        public override void AddRegiment<T>(HighlightRegiment regiment, List<T> units)
        {
            if (regiment.OwnerID != Manager.OwnerPlayerID) return;
            base.AddRegiment(regiment,units);
        }

        public override void AddRegiment(HighlightRegiment regiment, List<GameObject> units)
        {
            if (regiment.OwnerID != Manager.OwnerPlayerID) return;
            base.AddRegiment(regiment, units);
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Regiment Update Event ◈◈◈◈◈◈                                                                            ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void SwapDynamicToStatic()
        {
            CleanNullSelection();
            foreach (HighlightRegiment regiment in SelectedRegiments)
            {
                int regimentID = regiment.RegimentID;
                for (int i = 0; i < DynamicPlacementRegister.Records[regimentID].Length; i++)
                {
                    Vector3 position = DynamicPlacementRegister[regimentID][i].transform.localPosition;
                    Quaternion rotation = DynamicPlacementRegister[regimentID][i].transform.localRotation;
                    StaticPlacementRegister[regimentID][i].transform.SetLocalPositionAndRotation(position, rotation);
                }
            }
        }

        private void CleanNullSelection()
        {
            for (int i = SelectedRegiments.Count - 1; i > -1; i--)
            {
                int regimentID = SelectedRegiments[i].RegimentID;
                if(DynamicPlacementRegister.Records.ContainsKey(regimentID)) continue;
                SelectedRegiments.RemoveAt(i);
            }
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Rearrangement ◈◈◈◈◈◈                                                                                    ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        // When holding preview formation we want it to be updated when units die
        // We need to access Controller current Informations about temporary with used for those preview
        
        //TODO !!=====================================================================================================!!
        //TODO: CHECK IF STILL CORRECT WHEN FIRE STATE IS IMPLEMENTED!!!
        //TODO !!=====================================================================================================!!
        private (float3, FormationData) GetPlacementPreviewFormation(HighlightRegiment regiment, int numHighlightToKeep)
        {
            float3 offset = up() * 0.05f;
            
            if(PlacementController.SortedSelectedRegiments == null) return (regiment.CurrentPosition + offset, regiment.CurrentFormation);
            if(!PlacementController.SortedSelectedRegiments.TryGetIndexOf(regiment, out int indexSelection)) return (regiment.CurrentPosition + offset, regiment.CurrentFormation);
            
            //BUG!!!! Width can potentially have changed!
            if (PlacementController.DynamicsTempWidth != null)
            {
                //Debug.Log($"GetPlacementPreviewFormation : DynamicsTempWidth.Length = {PlacementController.DynamicsTempWidth.Length}, index = {indexSelection}");
                int tempWidth = PlacementController.DynamicsTempWidth is { Length: > 0 } ? PlacementController.DynamicsTempWidth[indexSelection] : regiment.TargetFormation.Width;
                //BUG: PlacementController.DynamicsTempWidth AND PlacementController.SortedSelectedRegiments are null 
                
                //Check par rapport au perte subi
                tempWidth = numHighlightToKeep < tempWidth ? numHighlightToKeep : tempWidth;
                //May fail here upon rearrangement
                
                int regimentID = regiment.RegimentID;
                float3 firstUnit = DynamicPlacementRegister[regimentID][0].transform.position;
                float3 lastUnit = DynamicPlacementRegister[regimentID][tempWidth-1].transform.position;
                
                //Get leader position in the placement preview
                float3 depthDirection = -normalizesafe(cross(up(), normalizesafe(lastUnit - firstUnit)));
                float3 leaderTempPosition = firstUnit + (lastUnit - firstUnit) / 2f;
                
                FormationData tempFormation = new (regiment.CurrentFormation,numHighlightToKeep, tempWidth, depthDirection);
                return (leaderTempPosition, tempFormation);
            }
            else
            {
                Debug.Log($"Use default");
                return (regiment.CurrentPosition + offset, regiment.CurrentFormation);
            }
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Resize ◈◈◈◈◈◈                                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        //BUG: Placement static placé incorrectement (Rotation oui bizarrement)
        protected override void ResizeAndReformRegister(int registerIndex, HighlightRegiment regiment, int numHighlightToKeep)
        {
            HighlightRegister register = Registers[registerIndex];
            if (!register.Records.ContainsKey(regiment.RegimentID)) return;
            HighlightBehaviour[] newRecordArray = register[regiment.RegimentID].Slice(0, numHighlightToKeep);
            
            if (numHighlightToKeep == 1)
            {
                newRecordArray[0].LinkToUnit(regiment.HighlightUnits[0].gameObject);
                newRecordArray[0].transform.position = regiment.transform.position;
            }
            else
            {
                for (int i = 0; i < numHighlightToKeep; i++)
                {
                    HighlightBehaviour highlight = newRecordArray[i];
                    highlight.LinkToUnit(regiment.HighlightUnits[i].gameObject); // ATTENTION, on part du principe que l'unité à été giclée
                    Vector3 position;
                    if (registerIndex == (int)EPlacementRegister.Dynamic)
                    {
                        (float3 leaderPos, FormationData tmpFormation) = GetPlacementPreviewFormation(regiment, numHighlightToKeep);
                        position = tmpFormation.GetUnitRelativePositionToRegiment3D(i, leaderPos);
                    }
                    else
                    {
                        position = regiment.TargetFormation.GetUnitRelativePositionToRegiment3D(i, regiment.TargetPosition);
                    }
                    highlight.transform.position = position;
                }
            }
            register[regiment.RegimentID] = newRecordArray;
        }
    }
}
