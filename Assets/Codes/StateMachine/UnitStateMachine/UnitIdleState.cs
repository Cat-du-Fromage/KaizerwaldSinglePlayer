using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

using Kaizerwald.FormationModule;
using Kaizerwald.Utilities;
using UnityEngine.InputSystem;

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
        public UnitIdleState(UnitStateMachine stateMachine) : base(stateMachine, EStates.Idle)
        {
            
        }

        public override void OnSetup(Order order)
        {
            return;
        }

        public override void OnEnter()
        {
            UnitAnimation.SetVelocity(0);
            //UnitAnimation.SetIdle();
        }

        public override void OnUpdate()
        {
            return;
        }

        public override void OnExit()
        {
            return;
        }

        public override bool ShouldExit(out EStates nextState)
        {
            if (RearrangeExit())
            {
                //Debug.Log($"Idle.RearrangeExit");
                nextState = EStates.Move;
            }
            else if (!IsRegimentStateIdentical && TryReturnToRegimentState(out EStates tmpNextState))
            {
                nextState = tmpNextState;
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
            float3 positionInFormation = CurrentFormationData.GetUnitRelativePositionToRegiment3D(IndexInFormation, LeaderPosition);
            float distanceToPosition = distancesq(Position, positionInFormation);
            bool isInPosition = distanceToPosition <= REACH_DISTANCE_THRESHOLD;
            
            //bool isInPosition = distanceToPosition <= 0.016f; //CANT USE REACH_DISTANCE_THRESHOLD dst calculated are differente from move state
            //if (!isInPosition || Keyboard.current.tKey.wasPressedThisFrame) Debug.Log($"RearrangeExit (isInPosition = {isInPosition}) : distanceToPosition = {distanceToPosition}");
            return !isInPosition;
        }
    }
}
