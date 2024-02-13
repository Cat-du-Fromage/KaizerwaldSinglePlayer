using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Kaizerwald.FormationModule;
using Kaizerwald.Utilities;

namespace Kaizerwald.StateMachine
{
    using RegimentState = StateBase<RegimentBehaviourTree>;
    
    [RequireComponent(typeof(Regiment))]
    public sealed class RegimentBehaviourTree : BehaviourTreeBase<RegimentBehaviourTree>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private Regiment linkedRegiment;
        private HashSet<UnitBehaviourTree> unitsBehaviourTrees;
        private HashSet<UnitBehaviourTree> deadUnitsBehaviourTrees;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public Regiment LinkedRegiment => linkedRegiment;
        public RegimentType RegimentType => linkedRegiment.RegimentType;
        //public RegimentFormationMatrix FormationMatrix => regimentAttach.RegimentFormationMatrix;

        public HashSet<UnitBehaviourTree> UnitsBehaviourTrees => unitsBehaviourTrees;
        
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
            foreach (RegimentState state in States.Values)
            {
                state?.OnDestroy();
            }
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void CleanUpNullUnitsStateMachine()
        {
            if (deadUnitsBehaviourTrees.Count == 0) return;
            unitsBehaviourTrees.ExceptWith(deadUnitsBehaviourTrees);
            deadUnitsBehaviourTrees.Clear();
        }
        
        public override void RequestChangeState(Order order)
        {
            base.RequestChangeState(order);// Propagate Order to Units
            foreach (UnitBehaviourTree unitBehaviourTree in unitsBehaviourTrees)
            {
                unitBehaviourTree.RequestChangeState(order);
            }
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                       ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public void InitializeAndRegisterUnits(Regiment regiment)
        {
            linkedRegiment = regiment;
            unitsBehaviourTrees = new HashSet<UnitBehaviourTree>(linkedRegiment.Count);
            deadUnitsBehaviourTrees = new HashSet<UnitBehaviourTree>(linkedRegiment.Count);
            
            InitializeStates();//Here must be done before init Units, because we need states reference in units state
            foreach (Unit unit in linkedRegiment.Units)
            {
                unit.BehaviourTree.Initialize(this);
                unitsBehaviourTrees.Add(unit.BehaviourTree);
            }
            IsInitialized = true;
        }
        
        protected override void InitializeStates()
        {
            States = new Dictionary<EStates, RegimentState>()
            {
                {EStates.Idle, new RegimentIdleState(this)},
                {EStates.Move, new RegimentMoveState(this)},
            };
            State = EStates.Idle; //CAREFULL was not set before? mistake or on purpose?
        }
    }
}
