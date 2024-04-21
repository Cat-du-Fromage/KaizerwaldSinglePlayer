using System;
using Kaizerwald.FormationModule;
using Kaizerwald.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;
using static Unity.Mathematics.math;

using static Kaizerwald.Utilities.KzwMath;
using static Kaizerwald.Utilities.CSharpContainerUtils;

namespace Kaizerwald.StateMachine
{
    public sealed class RegimentMoveState : RegimentStateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public readonly Transform RegimentTransform;
        
        public readonly float MarchSpeed;
        public readonly float RunSpeed;
        
        public readonly int MaxRange;
        
        private bool reachTargetPosition = true;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public RegimentBlackboard Blackboard => StateMachine.RegimentBlackboard;
        public float3 LeaderTargetPosition => LinkedRegiment.TargetPosition;
        public float CurrentSpeed => Blackboard.IsRunning ? RunSpeed : MarchSpeed;
    
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Getters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public bool LeaderReachTargetPosition => reachTargetPosition;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Setters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public RegimentMoveState(RegimentStateMachine stateMachine) : base(stateMachine,EStates.Move)
        {
            RegimentTransform = stateMachine.CachedTransform;
            MarchSpeed = stateMachine.RegimentType.MarchSpeed;
            RunSpeed = stateMachine.RegimentType.RunSpeed;
            MaxRange = stateMachine.RegimentType.Range;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public override bool ConditionEnter()
        {
            return true;
        }

        public override void OnSetup(Order order)
        {
            //Setup to correct rotation and forward position
            float3 center = Position - CurrentFormation.DirectionForward * (CurrentFormation.DistanceUnitToUnitY * (CurrentFormation.Depth - 1));
            float diameter = cmax(float2(CurrentFormation.WidthDepth - 1) * CurrentFormation.DistanceUnitToUnit);

            Vector3 startPosition = center + center.DirectionTo(order.TargetPosition) * (diameter / 2f);
            Quaternion startRotation = Quaternion.LookRotation(order.TargetFormation.Direction3DForward, Vector3.up);
            RegimentTransform.SetPositionAndRotation(startPosition, startRotation);
            
            LinkedRegiment.SetDestination(order.TargetPosition, order.TargetFormation);
            AssignIndexToUnits();
        }

        public override void OnEnter()
        {
            reachTargetPosition = false;
            UpdateProgressToTargetPosition();
            
            //TODO: make it change over time NOT instantly!
            CurrentFormation.SetFromFormation(TargetFormation);
        }
        /*
        public override void OnFixedUpdate()
        {
            if (reachTargetPosition) return;
            Vector3 position = MoveRegiment();
            Quaternion rotation = RotateRegiment();
            StateMachine.CachedTransform.SetPositionAndRotation(position, rotation);
        }
        */
        public override void OnUpdate()
        {
            if (reachTargetPosition) return;
            Vector3 position = MoveRegiment();
            Quaternion rotation = RotateRegiment();
            StateMachine.CachedTransform.SetPositionAndRotation(position, rotation);
            //RegimentTransform.SetPositionAndRotation(position, rotation);
        }

        public override void OnExit()
        {
            
        }

        public override bool ShouldExit(out EStates nextState)
        {
            nextState = GetExitState();
            return nextState != StateIdentity;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private EStates GetExitState()
        {
            if (Blackboard.HasTarget)
            {
                //Debug.Log($"Move.GetExitState: Blackboard.HasTarget = {Blackboard.EnemyTarget}");
                if (!RegimentManager.Instance.TryGetRegiment(Blackboard.EnemyTarget, out Regiment target))
                {
                    Blackboard.EnemyTarget = 0;
                    return StateMachine.DefaultState;
                }
                // xz necessary => bonus for high ground
                if (distance(Position.xz, target.Position.xz) < MaxRange) 
                {
                    return EStates.Fire;
                }
                else
                {
                    return StateIdentity;
                }
            }
            else
            {
                return reachTargetPosition ? StateMachine.DefaultState : StateIdentity;
            }
        }

        private void UpdateProgressToTargetPosition()
        {
            if (reachTargetPosition) return;
            float distanceToTarget = distance(Position, LeaderTargetPosition);
            //Debug.Log($"Move.UpdateProgressToTargetPosition: distanceToTarget = {distanceToTarget}");
            reachTargetPosition = distanceToTarget <= REACH_DISTANCE_THRESHOLD;
        }

        private Vector3 MoveRegiment()
        {
            if (reachTargetPosition) return Position; // Units may still be on their way
            
            float distanceMove = Time.deltaTime * CurrentSpeed;
            reachTargetPosition = distanceMove > distance(Position, LeaderTargetPosition);
            Vector3 translation = reachTargetPosition ? LeaderTargetPosition : Position + distanceMove * Position.DirectionTo(LeaderTargetPosition);
            return translation;
        }

        private Quaternion RotateRegiment()
        {
            return Quaternion.LookRotation(TargetFormation.DirectionForward, Vector3.up);
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ On Enter ◈◈◈◈◈◈                                                                                         ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private NativeArray<float> GetCostMatrix(in FormationData targetFormation, Allocator returnedAllocator)
        {
            NativeArray<float3> destinations = targetFormation.GetUnitsPositionRelativeToRegiment(LeaderTargetPosition, Temp);
            NativeArray<float> nativeCostMatrix = new (square(targetFormation.NumUnitsAlive), returnedAllocator, UninitializedMemory);
            for (int i = 0; i < nativeCostMatrix.Length; i++)
            {
                (int x, int y) = GetXY(i, targetFormation.NumUnitsAlive);
                float3 unitPosition = LinkedRegiment[y].Position;
                nativeCostMatrix[i] = distancesq(unitPosition, destinations[x]);
            }
            return nativeCostMatrix;
        }
    
        private void AssignIndexToUnits()
        {
            FormationData targetFormation = TargetFormation;
            using NativeArray<float> costMatrix = GetCostMatrix(targetFormation, TempJob);
            using NativeArray<int> sortedIndex = JobifiedHungarianAlgorithm.FindAssignments(costMatrix, targetFormation.NumUnitsAlive);
            LinkedRegiment.ReorderElementsBySwap(sortedIndex);
        }
    }
}