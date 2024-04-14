using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

using Kaizerwald.FormationModule;
using Kaizerwald.Utilities;

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
        private float3 LeaderPosition => LinkedParentRegiment.Position;
        private FormationData CurrentFormationData => LinkedParentRegiment.CurrentFormationData;
        
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

        public override bool ShouldExit(out EStates nextState)
        {
            if (RearrangeExit())
            {
                nextState = EStates.Move;
            }
            else if (!IsRegimentStateIdentical)
            {
                TryReturnToRegimentState(out nextState);
            }
            else
            {
                nextState = StateIdentity;
            }
            return nextState != StateIdentity;
        }
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public bool RearrangeExit()
        {
            
            float2 positionInFormation = CurrentFormationData.GetUnitRelativePositionToRegiment(IndexInFormation, LeaderPosition.xz);
            float distanceToPosition = distancesq(Position.xz, positionInFormation);
            bool isInPosition = distanceToPosition <= 0.016f; //CANT USE REACH_DISTANCE_THRESHOLD dst calculated are differente from move state
            
            //bool isInPosition = distanceToPosition <= REACH_DISTANCE_THRESHOLD;
            //if (!isInPosition) Debug.Log($"RearrangeExit (isInPosition = {isInPosition}) : distanceToPosition = {distanceToPosition}");
            return !isInPosition;
        }
    }
}
