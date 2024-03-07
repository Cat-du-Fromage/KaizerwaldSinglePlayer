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
        private float currentSpeed;
        private EMoveType currentMoveType;
        
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
        public bool IsAlreadyMoving => currentMoveType != EMoveType.None;
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
            //UnitReachTargetPosition = false;
            MoveOrder moveOrder = (MoveOrder)order;
            //float3 targetPosition = moveOrder.LeaderTargetPosition;
            //FormationData targetFormation = moveOrder.TargetFormation;
            //unitTargetPosition = targetFormation.GetUnitRelativePositionToRegiment3D(IndexInFormation, targetPosition);
            
            //TODO: remove this once automatic speed adaptation is implemented
            UpdateMoveType(moveOrder.MoveType); 
        }

        public override void OnEnter()
        {
            UnitReachTargetPosition = false;
            unitTargetPosition = TargetFormationData.GetUnitRelativePositionToRegiment3D(IndexInFormation, LeaderTargetPosition);
            UpdateProgressToTargetPosition();
            UpdateMoveSpeed();
            AdaptSpeed();
        }

        public override void OnUpdate()
        {
            if (UnitReachTargetPosition || LinkedUnit.IsInactive) return;
            
            unitTargetPosition = TargetFormationData.GetUnitRelativePositionToRegiment3D(IndexInFormation, LeaderTargetPosition);
            MoveUnit();
        }

        public override void OnExit()
        {
            currentMoveType = EMoveType.None;
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
        private bool IsDestinationReach()
        {
            return !IsRegimentStateIdentical && UnitReachTargetPosition && LeaderReachDestination;
        }

        private float3 GetDestinationInFormation()
        {
            FormationData formation = LinkedParentRegiment.TargetFormation;
            return TargetFormationData.GetUnitRelativePositionToRegiment3D(IndexInFormation, LeaderTargetPosition);
        }

        private bool IsDestinationReach()
        {
            if (UnitReachTargetPosition) return true;
            UnitReachTargetPosition = distancesq(Position, unitTargetPosition) <= REACH_DISTANCE_THRESHOLD;
            return UnitReachTargetPosition;
            //return distancesq(Position, unitTargetPosition) <= REACH_DISTANCE_THRESHOLD;
        }
        */
        private void UpdateMoveType(EMoveType moveType)
        {
            currentMoveType = moveType;
            currentSpeed = IsRunning ? RunSpeed : MarchSpeed;
        }
        
        private void UpdateProgressToTargetPosition()
        {
            if (UnitReachTargetPosition) return;
            UnitReachTargetPosition = distancesq(Position.xz, unitTargetPosition.xz) <= REACH_DISTANCE_THRESHOLD;
            
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
            if (!IsRunning || distancesq(Position, unitTargetPosition) > ADAPT_DISTANCE_THRESHOLD) return; 
            currentSpeed = SetMarching();
        }
    }
}
