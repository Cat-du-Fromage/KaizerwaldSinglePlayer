using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Kaizerwald.Utilities;
using Kaizerwald.FormationModule;
using Kaizerwald.StateMachine;

namespace Kaizerwald
{
    public sealed partial class Unit : FormationElementBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private Transform unitTransform;
        private Rigidbody unitRigidBody;
        private Collider unitCollider;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        // Components
        [field: SerializeField] public Regiment LinkedRegiment { get; private set; }
        [field: SerializeField] public UnitAnimation Animation { get; private set; }
        [field: SerializeField] public UnitBehaviourTree BehaviourTree { get; private set; }
        
        // Variables
        [field: SerializeField] public int IndexInFormation { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Getters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public Transform UnitTransform => unitTransform;
        public float3 Position => unitTransform.position;
        public float3 Forward  => unitTransform.forward;
        public float3 Back     => -unitTransform.forward;
        public float3 Right    => unitTransform.right;
        public float3 Left     => -unitTransform.right;
        public Quaternion Rotation => unitTransform.rotation;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Setters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void Awake()
        {
            unitTransform = transform;
            Animation = GetComponent<UnitAnimation>();
            unitRigidBody = GetComponent<Rigidbody>();
            unitCollider = GetComponent<Collider>();
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void UpdateUnit()
        {
            if (IsInactive) return;
            BehaviourTree.OnUpdate();
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Initialization Methods (Units are Initialize by their regiment) ◈◈◈◈◈◈                                  ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Static Constructor ◇◇◇◇◇◇                                                                          │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        // Meant to be instantiate this way AND NO OTHER WAY
        public static Unit InstantiateUnit(int indexInRegiment, int layerIndex, Regiment linkedRegiment, GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject unitGameObject = Instantiate(prefab, position, rotation);
            if (!unitGameObject.TryGetComponent(out Unit unit))
            {
                unit = unitGameObject.AddComponent<Unit>();
            }
            unit.Initialize(linkedRegiment, indexInRegiment, layerIndex);
            return unit;
        }
        
        public void Initialize(Regiment regiment, int indexInRegiment, int unitLayerIndex)
        {
            name = $"{name}_{indexInRegiment}";
            LinkedRegiment = regiment;
            gameObject.layer = unitLayerIndex;
            IndexInFormation = indexInRegiment;
            BehaviourTree = gameObject.GetOrAddComponent<UnitBehaviourTree>();
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Unit Event ◈◈◈◈◈◈                                                                                       ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public void TriggerDeath()
        {
            if (IsInactive) return;
            IsInactive = true;
            Animation.SetDead(); // need to track when animation finishes to disable collider
            LinkedRegiment.OnDeadUnit(this);
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ IFormationElement ◈◈◈◈◈◈                                                                                ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public void SetIndexInFormation(int newIndex) => IndexInFormation = newIndex;

        private void DestroyHighlight()
        {
            if (!TryGetComponent(out HighlightUnit highlightUnit)) return;
            highlightUnit.TriggerDeath();
        }
    
        public override void BeforeRemoval()
        {
            IsInactive = true;
        }

        public override void AfterRemoval()
        {
            //remove collider etc.. + inform highlight?
            //TODO: Find a way to link this event with Highlight.AfterRemoval
            DestroyHighlight();
            //unitCollider.enabled = false;
            //unitRigidBody.Sleep();
        }

        public override void OnRearrangement(int newIndex)
        {
            IndexInFormation = newIndex;
            if (IsInactive) return;
            MoveOrder moveOrder = new (LinkedRegiment.CurrentFormation, LinkedRegiment.TargetPosition, EMoveType.March);
            BehaviourTree.RequestChangeState(moveOrder);
        }
    }
}