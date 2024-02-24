using System;
using System.Collections.Generic;
using UnityEngine;

using Kaizerwald.FormationModule;
using Kaizerwald.Utilities;

namespace Kaizerwald
{
    public sealed class SelectionSystem : HighlightSystem
    {
        public enum ESelectionRegister
        {
            Preselection = 0,
            Selection = 1
        }
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public SelectionInfos SelectionInfos { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public List<HighlightRegiment> Regiments => Manager.RegimentsByPlayerID[Manager.OwnerPlayerID];
        
        //Add Remove Occure on Show/Hide base class of HighlightSystem (can't move it to register)
        public HighlightRegister PreselectionRegister => Registers[(int)ESelectionRegister.Preselection];
        public HighlightRegister SelectionRegister => Registers[(int)ESelectionRegister.Selection];
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public SelectionSystem(HighlightRegimentManager manager) : base(manager)
        {
            InitializeController();
            InitializeRegisters();
            SelectionInfos = new SelectionInfos(this);
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        protected override void InitializeController()
        {
            Controller = new SelectionController(this, Manager.HighlightControls, Manager.UnitLayerMask);
        }

        protected override void InitializeRegisters()
        {
            GameObject[] prefabs = { Manager.PreselectionDefaultPrefab, Manager.SelectionDefaultPrefab };
            for (int i = 0; i < prefabs.Length; i++)
            {
                Registers[i] = new HighlightRegister(this, prefabs[i]);
            }
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Behaviours Methods ◈◈◈◈◈◈                                                                               ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Register Regiment Tokens ◇◇◇◇◇◇                                                                    │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public override void AddRegiment(HighlightRegiment regiment, List<GameObject> units)
        {
            PreselectionRegister.RegisterRegiment(regiment, units);
            if (regiment.OwnerID != Manager.OwnerPlayerID) return;
            SelectionRegister.RegisterRegiment(regiment, units);
        }

        public override void RemoveRegiment(HighlightRegiment regiment)
        {
            base.RemoveRegiment(regiment);
        }

        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Show | Hide ◇◇◇◇◇◇                                                                                 │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public override void OnShow(HighlightRegiment regiment, int registerIndex)
        {
            regiment.SetSelectableProperty((ESelectionRegister)registerIndex, true);
            base.OnShow(regiment, registerIndex);
            
            if ((ESelectionRegister)registerIndex != ESelectionRegister.Selection) return;
            SelectionInfos.OnSelectionUpdate();
        }
        
        public override void OnHide(HighlightRegiment regiment, int registerIndex)
        {
            regiment.SetSelectableProperty((ESelectionRegister)registerIndex, false);
            base.OnHide(regiment, registerIndex);
            
            if ((ESelectionRegister)registerIndex != ESelectionRegister.Selection) return;
            SelectionInfos.OnSelectionUpdate();
        }

        public override void HideAll(int registerIndex)
        {
            foreach (HighlightRegiment regiment in Registers[registerIndex].ActiveHighlights)
            {
                regiment.SetSelectableProperty((ESelectionRegister)registerIndex, true);
            }
            base.HideAll(registerIndex);
            
            if ((ESelectionRegister)registerIndex != ESelectionRegister.Selection) return;
            SelectionInfos.OnSelectionUpdate();
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Rearrangement ◈◈◈◈◈◈                                                                                    ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        protected override void ResizeAndReformRegister(int registerIndex, HighlightRegiment regiment, int numHighlightToKeep)
        {
            if (!Registers[registerIndex].TryGetValue(regiment.RegimentID, out HighlightBehaviour[] highlights)) return;
            HighlightBehaviour[] newRecordArray = highlights.Slice(0, numHighlightToKeep);
            for (int i = 0; i < numHighlightToKeep; i++)
            {
                newRecordArray[i].LinkToUnit(regiment.HighlightUnits[i].gameObject);
            }
            Registers[registerIndex][regiment.RegimentID] = newRecordArray;
        }
        
        //Pour l'instant Uniquement sur séléction, Placement bug autrement!
        protected override void CleanUnusedHighlights(int registerIndex, HighlightRegiment regiment, int numToKeep)
        {
            base.CleanUnusedHighlights(registerIndex, regiment, numToKeep);
            SelectionInfos.OnSelectionUpdate(); //seems to not update fast enough sometimes....
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Orders Callback ◈◈◈◈◈◈                                                                              ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        //public void OnAttackOrderEvent() { }
    }
}