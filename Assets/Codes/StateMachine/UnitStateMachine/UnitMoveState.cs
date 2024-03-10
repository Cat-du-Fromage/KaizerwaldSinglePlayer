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
        public const float ADAPT_DISTANCE_THRESHOLD = 0.125f; //was 0.5f
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private float3 unitTargetPosition;
        private float previousSpeed;
        private float currentSpeed;
        //private EMoveType currentMoveType;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        public bool UnitReachTargetPosition { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        // RegimentStateReference
        public bool LeaderReachDestination => RegimentStateReference.LeaderReachTargetPosition;
        public float3 LeaderTargetPosition => RegimentStateReference.LeaderTargetPosition;
        public FormationData TargetFormationData => RegimentStateReference.TargetFormation;
        
        public float MarchSpeed => RegimentStateReference.MarchSpeed;
        public float RunSpeed => RegimentStateReference.RunSpeed;
        
        //UnitState
        //public bool IsAlreadyMoving => currentMoveType != EMoveType.None;
        //public bool IsRunning => currentMoveType == EMoveType.Run;
        
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
            //MoveOrder moveOrder = (MoveOrder)order;
            //float3 targetPosition = moveOrder.LeaderTargetPosition;
            //FormationData targetFormation = moveOrder.TargetFormation;
            //unitTargetPosition = targetFormation.GetUnitRelativePositionToRegiment3D(IndexInFormation, targetPosition);
            
            //TODO: remove this once automatic speed adaptation is implemented
            //UpdateMoveType(moveOrder.MoveType); 
        }

        public override void OnEnter()
        {
            currentSpeed = previousSpeed = 0;
            UnitReachTargetPosition = false;
            unitTargetPosition = TargetFormationData.GetUnitRelativePositionToRegiment3D(IndexInFormation, LeaderTargetPosition);
            UpdateProgressToTargetPosition();
            //UnitAnimation.SetSpeed(RegimentStateReference.CurrentSpeed);
            AdaptSpeed();
        }

        public override void OnUpdate()
        {
            if (LinkedUnit.IsInactive) return;
            unitTargetPosition = TargetFormationData.GetUnitRelativePositionToRegiment3D(IndexInFormation, LeaderTargetPosition);
            UpdateProgressToTargetPosition();
            if (UnitReachTargetPosition)
            {
                if (UnitAnimation.IsPlayingIdle) return;
                UnitAnimation.SetIdle();
                currentSpeed = previousSpeed = 0;
            }
            else
            {
                MoveUnit();
            }
        }

        public override void OnExit()
        {
            //currentMoveType = EMoveType.None;
        }

        public override bool ShouldExit(out EStates nextState)
        {
            UpdateProgressToTargetPosition();
            if (!IsRegimentStateIdentical && UnitReachTargetPosition && LeaderReachDestination)
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
//TODO : Function that adapt speed depending on the distance from destination to current leader Position(GetUnitRelativePositionToRegiment3D), NOT final target position (unitTargetPosition)
        /*
        private void UpdateMoveType(EMoveType moveType)
        {
            currentMoveType = moveType;
            currentSpeed = IsRunning ? RunSpeed : MarchSpeed;
        }
        */
        private void UpdateProgressToTargetPosition()
        {
            UnitReachTargetPosition = distancesq(Position.xz, unitTargetPosition.xz) <= REACH_DISTANCE_THRESHOLD;
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ State Logic ◈◈◈◈◈◈                                                                                      ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        /*
        private void UpdateMoveSpeed()
        {
            currentSpeed = IsRunning ? SetRunning() : SetMarching();
        }
        */
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

        //TODO: Find a way to make Run speed == Animation Running speed
        private void UpdateBaseSpeed()
        {
            currentSpeed = RegimentStateReference.CurrentSpeed;
            if (!currentSpeed.IsAlmostEqual(previousSpeed))
            {
                float animationSpeed = remap(MarchSpeed, RunSpeed, 2, 6, currentSpeed);
                UnitAnimation.SetSpeed(animationSpeed);
            }
            previousSpeed = currentSpeed;
        }

        private void MoveUnit()
        {
            UpdateBaseSpeed();
            AdaptSpeed();
            
            float3 translation = Time.deltaTime * currentSpeed * Position.DirectionTo(unitTargetPosition);
            UnitTransform.Translate(translation, Space.World);
            UnitTransform.LookAt(Position + RegimentStateReference.TargetFormation.DirectionForward);
            
        }

        private void AdaptSpeed()
        {
            if (currentSpeed.IsAlmostEqual(MarchSpeed) || distancesq(Position.xz, unitTargetPosition.xz) > ADAPT_DISTANCE_THRESHOLD) return; 
            currentSpeed = SetMarching();
        }
    }
}
