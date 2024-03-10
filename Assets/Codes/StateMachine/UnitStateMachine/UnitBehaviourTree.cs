using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kaizerwald.StateMachine
{
    using UnitState = StateBase<UnitBehaviourTree>;
    
    [RequireComponent(typeof(Unit))]
    public sealed class UnitBehaviourTree : BehaviourTreeBase<UnitBehaviourTree>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private Unit linkedUnit;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public Unit LinkedUnit => linkedUnit;
        public RegimentBehaviourTree RegimentBehaviourTree { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public EStates RegimentState => RegimentBehaviourTree.State;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected override void Awake()
        {
            base.Awake();
            linkedUnit = GetComponent<Unit>();
        }

        public override void OnUpdate()
        {
            if (!IsInitialized) return;
            base.OnUpdate();
        }

        public void OnDestroy()
        {
            RegimentBehaviourTree.OnUnitDestroyed(this);
            if (States == null) return;
            foreach (UnitState state in States.Values)
            {
                state?.OnDestroy();
            }
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public UnitBehaviourTree Initialize(RegimentBehaviourTree parentBehaviourTree)
        {
            RegimentBehaviourTree = parentBehaviourTree;
            InitializeStates();// MUST BE DONE LAST! because we need RegimentBehaviourTree to be assigned first!
            IsInitialized = true;
            return this;
        }

        public override void RequestChangeState(Order order, bool overrideState = true)
        {
            if (!States[order.StateOrdered].ConditionEnter()) return;
            base.RequestChangeState(order, overrideState);
        }

        protected override void InitializeStates()
        {
            States = new Dictionary<EStates, UnitState>()
            {
                {EStates.Idle, new UnitIdleState(this)},
                {EStates.Move, new UnitMoveState(this)},
                {EStates.Fire, new UnitRangeAttackState(this)},
            };
            State = EStates.Idle;
        }
    }
}
