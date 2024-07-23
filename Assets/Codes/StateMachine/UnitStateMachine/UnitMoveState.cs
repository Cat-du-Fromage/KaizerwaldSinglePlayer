using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

using Kaizerwald.FormationModule;
using Kaizerwald.Utilities.Core;

using quaternion = Unity.Mathematics.quaternion;

namespace Kaizerwald.StateMachine
{
    public sealed class UnitMoveState : UnitStateBase<RegimentMoveState>
    {
        public const float ADAPT_DISTANCE_THRESHOLD = 0.125f; //was 0.5f
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private float3 unitCurrentTargetPosition;
        private float3 unitFinalTargetPosition;

        private float acceleration = 0;
        //private float distanceMultiplier = 0;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public bool2 UnitReachTargetPosition;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public Rigidbody UnitRigidBody => LinkedUnit.UnitRigidBody;
    
        // RegimentState Reference
        public bool LeaderReachDestination => RegimentStateReference.LeaderReachTargetPosition;
        public float3 LeaderTargetPosition => RegimentStateReference.LeaderTargetPosition;
        public FormationData TargetFormationData => RegimentStateReference.TargetFormation;
        
        // March Speed Accessors
        public float MarchSpeed => RegimentStateReference.MarchSpeed;
        public float RunSpeed => RegimentStateReference.RunSpeed;
        
        private float CurrentSpeed => RegimentStateReference.CurrentSpeed;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public UnitMoveState(UnitStateMachine stateMachine) : base(stateMachine, EStates.Move)
        {
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public override bool ConditionEnter() { return true; }

        public override void OnSetup(Order order) { return; }

        public override void OnEnter()
        {
            acceleration = 0;
            UnitReachTargetPosition = false;
            unitFinalTargetPosition = TargetFormationData.GetUnitRelativePositionToRegiment3D(IndexInFormation, LeaderTargetPosition);
            unitCurrentTargetPosition = unitFinalTargetPosition;
            UpdateProgressToTargetPosition();
        }

        public override void OnFixedUpdate()
        {
            if (LinkedUnit.IsInactive) return;
            
            unitFinalTargetPosition = TargetFormationData.GetUnitRelativePositionToRegiment3D(IndexInFormation, LeaderTargetPosition);
            
            unitCurrentTargetPosition = LeaderReachDestination 
                ? unitFinalTargetPosition 
                : TargetFormationData.GetUnitRelativePositionToRegiment3D(IndexInFormation, LinkedParentRegiment.Position);
            
            UpdateProgressToTargetPosition();
            if (all(UnitReachTargetPosition))
            {
                //if (UnitAnimation.IsPlayingIdle || IsRegimentStateIdentical) return;
                //if (LeaderReachDestination && !IsRegimentStateIdentical) return;
                UnitAnimation.SetVelocity(0);
            }
            else
            {
                bool doRotate = RotateEntity(out Quaternion newRotation);
                bool doMove = MoveEntity(out Vector3 newPosition);
                UnitRigidBody.Move(newPosition, newRotation);
                SetMoveAnimation();
            }

            //DebugMoveState();
        }
        
        public override void OnUpdate()
        {
            return;
        }

        public override void OnExit()
        {
            acceleration = 0;
            UnitAnimation.SetVelocity(0);
        }

        public override bool ShouldExit(out EStates nextState)
        {
            UpdateProgressToTargetPosition();
            if (LeaderReachDestination && !IsRegimentStateIdentical && all(UnitReachTargetPosition) && TryReturnToRegimentState(out EStates tmpNextState))
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
//TODO : Function that adapt speed depending on the distance from destination to current leader Position(GetUnitRelativePositionToRegiment3D), NOT final target position (unitTargetPosition)
        private void UpdateProgressToTargetPosition()
        {
            bool positionCheck = distancesq(Position, unitFinalTargetPosition) <= REACH_DISTANCE_THRESHOLD;
            
            int2 currentForward = (int2)ceil(Forward.xz * 100);
            int2 forwardTarget = (int2)ceil(RegimentStateReference.TargetFormation.DirectionForward.xz * 100);
            bool rotationCheck = all(abs(currentForward - forwardTarget) <= 1);
            
            UnitReachTargetPosition = new bool2(positionCheck, rotationCheck);
            //if (!UnitReachTargetPosition[0] && !IsRegimentStateIdentical) Debug.Log($"Position = {Position}, unitFinalTargetPosition = {unitFinalTargetPosition}");
            //if (!UnitReachTargetPosition[1] && !IsRegimentStateIdentical) Debug.Log($"currentForward = {currentForward}, forwardTarget = {forwardTarget}");
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ State Logic ◈◈◈◈◈◈                                                                                      ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private bool RotateEntity(out Quaternion newRotation)
        {
            newRotation = Rotation;
            if (UnitReachTargetPosition[1]) return false;
            Quaternion targetRotation = Quaternion.LookRotation(RegimentStateReference.TargetFormation.DirectionForward, Vector3.up);
            newRotation = Quaternion.Slerp(Rotation, targetRotation, 1);
            //UnitRigidBody.MoveRotation(newRotation);
            return true;
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Move Logic ◇◇◇◇◇◇                                                                                  │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

        private void SetMoveAnimation()
        {
            float mappedVelocity = remap(MarchSpeed, RunSpeed, 1, 2, CurrentSpeed);
            UnitAnimation.SetVelocity(mappedVelocity);
        }
        
        private bool MoveEntity(out Vector3 newPosition)
        {
            newPosition = Position;
            if (UnitReachTargetPosition[0]) return false;
            
            bool isNearFinalTarget = Position.distance(unitFinalTargetPosition) <= CurrentSpeed * Time.fixedDeltaTime;
            
            float3 direction = Position.direction(unitCurrentTargetPosition);
            
            newPosition = isNearFinalTarget ? unitFinalTargetPosition : Position + direction * CurrentSpeed * Time.fixedDeltaTime;
            return true;
            //UnitRigidBody.MovePosition(isNearFinalTarget ? unitFinalTargetPosition : newPosition);
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Debug Utils ◇◇◇◇◇◇                                                                                 │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void DebugMoveState()
        {
            if (IsRegimentStateIdentical) return;
            //Debug.Log($"position = {UnitReachTargetPosition.x}, rotation = {UnitReachTargetPosition.y}");
            if (!UnitReachTargetPosition.x)
            {
                //Debug.Log($"POSITION : unitFinalTargetPosition = {unitFinalTargetPosition}, unitCurrentTargetPosition = {unitCurrentTargetPosition}");
            }
            if (!UnitReachTargetPosition.y)
            {
                int2 currentForward = (int2)ceil(Forward.xz * 100);
                int2 forwardTarget = (int2)ceil(RegimentStateReference.TargetFormation.DirectionForward.xz * 100);
                int2 diff = currentForward - forwardTarget;
                //Debug.Log($"ROTATION : diff = {diff} currentForward = {currentForward}/{Forward.xz}, forwardTarget = {forwardTarget}/{RegimentStateReference.TargetFormation.DirectionForward.xz} = {all(currentForward == forwardTarget)}");
            }
        }
    }
}
