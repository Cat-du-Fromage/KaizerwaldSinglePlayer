using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Kaizerwald.Utilities.Core;
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
        [field: SerializeField] public UnitStateMachine StateMachine { get; private set; }
        
        // Variables
        [field: SerializeField] public int IndexInFormation { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Getters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public Rigidbody UnitRigidBody => unitRigidBody;
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

        public void OnFixedUpdate()
        {
            if (IsInactive) return;
            StateMachine.OnFixedUpdate();
        }

        public void OnUpdate()
        {
            if (IsInactive) return;
            StateMachine.OnUpdate();
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Initialization Methods (Units are Initialize by their regiment) ◈◈◈◈◈◈                                  ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Static Constructor ◇◇◇◇◇◇                                                                          │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        // Meant to be instantiate this way AND NO OTHER WAY
        public static Unit InstantiateUnit(Regiment linkedRegiment, GameObject prefab, Vector3 position, Quaternion rotation, int indexInRegiment, int layerIndex)
        {
            GameObject unitGameObject = Instantiate(prefab, position, rotation);
            if (!unitGameObject.TryGetComponent(out Unit unit))
            {
                unit = unitGameObject.AddComponent<Unit>();
            }
            return unit.Initialize(linkedRegiment, indexInRegiment, layerIndex);
        }
        
        public Unit Initialize(Regiment regiment, int indexInRegiment, int unitLayerIndex)
        {
            name = $"{name}_{indexInRegiment}";
            LinkedRegiment = regiment;
            gameObject.layer = unitLayerIndex;
            IndexInFormation = indexInRegiment;
            StateMachine = gameObject.GetOrAddComponent<UnitStateMachine>();
            return this;
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
            StartCoroutine(DisableCollider());
        }
        
        private IEnumerator DisableCollider()
        {
            yield return new WaitForSeconds(3f);
            unitCollider.enabled = false;
            unitRigidBody.detectCollisions = false;
            unitRigidBody.Sleep();
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
            Destroy(StateMachine);
        }

        public override void AfterRemoval()
        {
            //remove collider etc.. + inform highlight?
            //TODO: Find a way to link this event with Highlight.AfterRemoval
            DestroyHighlight();
        }
        
        public override void OnRearrangement(int newIndex)
        {
            IndexInFormation = newIndex;
            //if (IsInactive) return;
            //MoveOrder moveOrder = new (LinkedRegiment.CurrentFormation, LinkedRegiment.TargetPosition, EMoveType.March);
            //BehaviourTree.RequestChangeState(moveOrder, false);
        }
    }
}