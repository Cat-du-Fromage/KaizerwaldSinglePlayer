using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaizerwald.FormationModule;
using Kaizerwald.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Kaizerwald
{
    public sealed class HighlightRegiment : UnorderedFormationBehaviour<HighlightUnit>
    {
        [field:SerializeField] public ulong OwnerID { get; private set; }
        [field:SerializeField] public int TeamID { get; private set; }
        [field:SerializeField] public int RegimentID { get; private set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private Transform regimentTransform;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public float3 CurrentPosition => regimentTransform.position;
        public List<HighlightUnit> HighlightUnits => Elements;
        public List<Transform> HighlightTransforms => Transforms;
        public TransformAccessArray HighlightUnitsTransform => FormationTransformAccessArray;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ ISelectable ◈◈◈◈◈◈                                                                                      ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private bool2 selectableStates = false;
        public bool IsPreselected => selectableStates[0];
        public bool IsSelected => selectableStates[1];
        
        public void SetSelectableProperty(SelectionSystem.ESelectionRegister selectionRegister, bool value)
        {
            selectableStates[(int)selectionRegister] = value;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void Awake()
        {
            regimentTransform = transform;
        }

        public void OnFixedUpdate()
        {
            UpdateFormation();
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public void InitializeHighlight(ulong ownerID, int teamID, List<GameObject> units, int2 minMaxRow, float2 unitSize, float spaceBetweenUnit, float3 direction)
        {
            RegimentID = transform.GetInstanceID();
            OwnerID = ownerID;
            TeamID = teamID;
            Formation formation = new Formation(units.Count, minMaxRow, unitSize, spaceBetweenUnit, direction);
            List<HighlightUnit> highlightUnits = units.Select(RegisterHighlightUnit).ToList();
            
            InitializeFormation(formation, highlightUnits, CurrentPosition);
        }
        
        public void InitializeHighlight(ulong ownerID, int teamID, List<GameObject> units, FormationData data)
        {
            InitializeHighlight(ownerID, teamID, units, data.MinMaxRow, data.UnitSize, data.SpaceBetweenUnits, data.Direction3DForward);
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Add | Remove ◈◈◈◈◈◈                                                                                     ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public HighlightUnit RegisterHighlightUnit(GameObject unit)
        {
            HighlightUnit highlightUnit = unit.AddComponent<HighlightUnit>();
            highlightUnit.AttachToRegiment(this);
            return highlightUnit;
        }

        public HighlightUnit RegisterHighlightUnit<T>(T unit) where T : MonoBehaviour, IFormationElement
        {
            return RegisterHighlightUnit(unit.gameObject);
        }
    }
}
