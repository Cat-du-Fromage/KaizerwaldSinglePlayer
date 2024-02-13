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
    public sealed class UnitMoveState : UnitStateBase<RegimentMoveState>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private float3 unitTargetPosition;
        private float currentSpeed;
        private EMoveType currentMoveType;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        public bool UnitReachTargetPosition { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        //RegimentBehaviour
        public bool LeaderReachDestination => RegimentStateReference.LeaderReachTargetPosition;
        public float3 LeaderTargetPosition => RegimentStateReference.LeaderTargetPosition;
        public float MarchSpeed => RegimentStateReference.MarchSpeed;
        public float RunSpeed => RegimentStateReference.RunSpeed;
        
        //UnitState
        public bool IsRunning => currentMoveType == EMoveType.Run;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Setters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public UnitMoveState(UnitBehaviourTree behaviourTree) : base(behaviourTree, EStates.Move)
        {
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public override void OnSetup(Order order)
        {
            UnitReachTargetPosition = false;
            MoveOrder moveOrder = (MoveOrder)order;
            FormationData targetFormation = moveOrder.TargetFormation;
            float3 leaderTargetPosition = moveOrder.LeaderTargetPosition;
            UpdateMoveType(moveOrder.MoveType);
            unitTargetPosition = targetFormation.GetUnitRelativePositionToRegiment3D(IndexInFormation, leaderTargetPosition);
        }

        public override void OnEnter()
        {
            UpdateProgressToTargetPosition();
            UpdateMoveSpeed();
            AdaptSpeed();
        }

        public override void OnUpdate()
        {
            if (UnitReachTargetPosition || LinkedUnit.IsInactive) return;
            MoveUnit();
        }

        public override void OnExit()
        {
            UnitReachTargetPosition = false;
        }

        public override EStates ShouldExit()
        {
            UpdateProgressToTargetPosition();
            return TryReturnToRegimentState();
        }
        
        protected override EStates TryReturnToRegimentState()
        {
            if (StateIdentity == RegimentState || !UnitReachTargetPosition || !LeaderReachDestination)
            {
                return StateIdentity;
            }
            bool canEnterNextState = BehaviourTree.States[RegimentState].ConditionEnter();
            EStates nextState = canEnterNextState ? RegimentState : DefaultNextState;
            return nextState;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void UpdateMoveType(EMoveType moveType)
        {
            currentMoveType = moveType;
            currentSpeed = IsRunning ? RunSpeed : MarchSpeed;
        }
        
        private void UpdateProgressToTargetPosition()
        {
            if (UnitReachTargetPosition) return;
            UnitReachTargetPosition = distancesq(Position, unitTargetPosition) <= 0.0125f;
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ State Logic ◈◈◈◈◈◈                                                                                      ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private void UpdateMoveSpeed()
        {
            currentSpeed = IsRunning ? SetRunning() : SetMarching();
        }
    
        private float SetMarching()
        {
            UnitAnimation.SetMarching();
            return MarchSpeed;
        }

        private float SetRunning()
        {
            UnitAnimation.SetRunning();
            return RunSpeed;
        }

        private void MoveUnit()
        {
            AdaptSpeed();
            float3 translation = Time.deltaTime * currentSpeed * Position.DirectionTo(unitTargetPosition);
            UnitTransform.Translate(translation, Space.World);
            UnitTransform.LookAt(Position + RegimentStateReference.TargetFormation.DirectionForward);
        }

        private void AdaptSpeed()
        {
            const float threshold = 0.5f; //0.125f
            if (!IsRunning || distancesq(Position, unitTargetPosition) > threshold) return; 
            currentSpeed = SetMarching();
        }
    }
}
