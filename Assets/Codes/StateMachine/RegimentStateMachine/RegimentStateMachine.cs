using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Kaizerwald.FormationModule;
using Kaizerwald.Utilities;

namespace Kaizerwald.StateMachine
{
    using RegimentBaseState = StateBase<RegimentStateMachine>;
    
    [RequireComponent(typeof(Regiment))]
    public sealed class RegimentStateMachine : StateMachineBase<RegimentStateMachine>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private Regiment linkedRegiment;
        private HashSet<UnitStateMachine> unitsStateMachines;
        private HashSet<UnitStateMachine> deadUnitsBehaviourTrees;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public RegimentBlackboard RegimentBlackboard { get; private set; } = new RegimentBlackboard();

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public Regiment LinkedRegiment => linkedRegiment;
        public RegimentType RegimentType => linkedRegiment.RegimentType;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝ 

        public override void OnUpdate()
        {
            if (!IsInitialized) return;
            CleanUpNullUnitsStateMachine();
            base.OnUpdate();
        }

        public void OnDestroy()
        {
            if (States == null) return;
            foreach (RegimentBaseState state in States.Values)
            {
                state?.OnDestroy();
            }
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void OnUnitDestroyed(UnitStateMachine unit)
        {
            deadUnitsBehaviourTrees.Add(unit);
        }

        private void CleanUpNullUnitsStateMachine()
        {
            if (deadUnitsBehaviourTrees.Count == 0) return;
            unitsStateMachines.ExceptWith(deadUnitsBehaviourTrees);
            deadUnitsBehaviourTrees.Clear();
        }

        public override void RequestChangeState(Order order)
        {
            CleanUpNullUnitsStateMachine();
            RegimentBlackboard.EnemyTarget = order.EnemyTargetId;
            base.RequestChangeState(order);
        }

        public override void RequestChangeState(Order order, bool overrideState)
        {
            CleanUpNullUnitsStateMachine();
            RegimentBlackboard.EnemyTarget = order.EnemyTargetId;
            base.RequestChangeState(order, overrideState);// Propagate Order to Units
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public RegimentStateMachine InitializeAndRegisterUnits(Regiment regiment)
        {
            linkedRegiment = regiment;
            unitsStateMachines = new HashSet<UnitStateMachine>(linkedRegiment.Count);
            deadUnitsBehaviourTrees = new HashSet<UnitStateMachine>(linkedRegiment.Count);
            
            InitializeStates(); // Here must be done before init Units, because we need states reference in units state
            foreach (Unit unit in linkedRegiment.Elements)
            {
                unitsStateMachines.Add(unit.StateMachine.Initialize(this));
            }
            IsInitialized = true;
            return this;
        }
        
        protected override void InitializeStates()
        {
            States = new Dictionary<EStates, RegimentBaseState>()
            {
                {EStates.Idle, new RegimentIdleState(this)},
                {EStates.Move, new RegimentMoveState(this)},
                {EStates.Fire, new RegimentRangeAttackState(this)},
            };
            State = EStates.Idle; //CAREFULL was not set before? mistake or on purpose?
        }
    }
}
