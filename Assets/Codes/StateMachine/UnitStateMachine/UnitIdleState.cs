using System;
using System.Collections;
using System.Collections.Generic;
using Kaizerwald.FormationModule;
using Unity.Mathematics;
using UnityEngine;

namespace Kaizerwald.StateMachine
{
    public sealed class UnitIdleState : UnitStateBase<RegimentIdleState>
    {
        //public const float REACH_DISTANCE_THRESHOLD = 0.0125f;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public UnitIdleState(UnitBehaviourTree behaviourTree) : base(behaviourTree, EStates.Idle)
        {
            
        }

        public override void OnSetup(Order order)
        {
            return;
        }

        public override void OnEnter()
        {
            UnitAnimation.SetIdle();
        }

        public override void OnUpdate()
        {
            //Rien?
        }

        public override void OnExit()
        {
            //Rien?
        }

        public override EStates ShouldExit()
        {
            //if (FireExit()) return EStates.Fire;
            return TryReturnToRegimentState();
        }
        
        protected override EStates TryReturnToRegimentState()
        {
            if (IsRegimentStateIdentical) return StateIdentity;
            
            return BehaviourTree.States[RegimentState].ConditionEnter() ? RegimentState : StateIdentity;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    }
}
