using System;
using System.Collections.Generic;
using Kaizerwald.FormationModule;
using Kaizerwald.TerrainBuilder;
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

using Kaizerwald.Utilities.Core;
using static Kaizerwald.Utilities.Core.KzwMath;
using static Kaizerwald.Utilities.Core.KzwGrid;
using static Kaizerwald.Utilities.Core.ArrayExtension;

namespace Kaizerwald.StateMachine
{
    public sealed class RegimentMoveState : RegimentStateBase
    {
        private const float REACH_DISTANCE_THRESHOLD = 0.0125f;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public readonly Transform RegimentTransform;
        
        public readonly float MarchSpeed;
        public readonly float RunSpeed;
        
        public readonly int MaxRange;
        
        private bool reachTargetPosition = true;

        private Cell[] currentPath = Array.Empty<Cell>();
        
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

        public override bool ConditionEnter() => true;

        public override void OnSetup(Order order)
        {
            //Setup to initial rotation and forward position
            //Vector3 startPosition = GetStartPosition2(order);
            
            float3 startPosition = GetStartPosition(order).x0y();
            Quaternion startRotation = Quaternion.LookRotation(order.TargetFormation.Direction3DForward, Vector3.up);
            RegimentTransform.SetPositionAndRotation(startPosition, startRotation);
            
            // WILL CHANGE!
            LinkedRegiment.SetDestination(order.TargetPosition, order.TargetFormation);
            AssignIndexToUnits();
        }

        public override void OnEnter()
        {
            CalculatePath();
            
            reachTargetPosition = false;
            UpdateProgressToTargetPosition();
            
            //TODO: make it change over time NOT instantly!
            CurrentFormation.CopyFrom(TargetFormation);
            //CurrentFormation.SetFromFormation(TargetFormation);
        }
        
        public override void OnUpdate()
        {
            if (reachTargetPosition) return;
            
            //Decomposition en 2 étapes
            // 1) rotation Initial et/ou flip
            // les troupes font uniquement la rotation
            // BUT: quand les troupes sont correctement placée => rotation plus position initiale
            
            Vector3 position = MoveRegiment();
            Quaternion rotation = RotateRegiment();
            
            StateMachine.CachedTransform.SetPositionAndRotation(position, rotation);
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
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ On Setup ◈◈◈◈◈◈                                                                                         ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private Vector3 GetStartPosition2(Order order)
        {
            float3 center = Position - CurrentFormation.DirectionForward * (CurrentFormation.DistanceUnitToUnit.y * (CurrentFormation.Depth - 1));
            float diameter = cmax(float2(CurrentFormation.WidthDepth - 1) * CurrentFormation.DistanceUnitToUnit);
            return center + center.direction(order.TargetPosition) * (diameter / 2f);
        }
    
        private int GetClosestUnitIndexFromTarget(float2 target)
        {
            int count = CurrentFormation.UnitCount;
            int halfCount = count / 2;
            (int minIndex, float minDistance) = (halfCount, distance(LinkedRegiment[halfCount].Position.xz, target));
            (int minStartEndIndex, float minDistanceStartEnd) = (0, float.MaxValue);
            for (int i = 0; i < halfCount; i++)
            {
                float distanceStart = distance(LinkedRegiment[i].Position.xz, target);
                float distanceEnd   = distance(LinkedRegiment[^(1+i)].Position.xz, target);
                
                bool isStartMin = distanceStart < distanceEnd;
                (minStartEndIndex, minDistanceStartEnd) = isStartMin ? (i, distanceStart) : (count - (1 + i), distanceEnd);
                bool swapMin = minDistanceStartEnd < minDistance;
                (minIndex, minDistance) = swapMin ? (minStartEndIndex, minDistanceStartEnd) : (minIndex, minDistance);
            }
            return minIndex;
        }
        
        private int GetClosestUnitIndexFromTarget2(float2 target)
        {
            (int minIndex, float minDistance) = (0, distance(LinkedRegiment[0].Position.xz,target));
            for (int i = 1; i < CurrentFormation.UnitCount; i++)
            {
                float distance = LinkedRegiment[i].Position.xz.distance(target);
                (minIndex, minDistance) = distance < minDistance ? (i, distance) : (minIndex, minDistance);
            }
            return minIndex;
        }
    
        private float2 GetStartPosition(Order order)
        {
            float2 position2D = Position.xz;
            
            int closestUnitIndex = GetClosestUnitIndexFromTarget(order.TargetPosition.xz);
            float2 unitPosition = LinkedRegiment[closestUnitIndex].Position.xz;
            
            //2) Segment position to target
            float2 positionToTarget = order.TargetPosition.xz - position2D;
            
            //3) Projection Closest Unit To Segment
            float2 positionToUnit = unitPosition - position2D;
            return position2D + project(positionToUnit, positionToTarget);
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ On Enter ◈◈◈◈◈◈                                                                                         ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private NativeArray<float> GetCostMatrix(in FormationData targetFormation)
        {
            int unitCount = targetFormation.UnitCount;
            NativeArray<float3> destinations = ((Formation)targetFormation).GetPositions(LeaderTargetPosition, Temp);
            NativeArray<float> nativeCostMatrix = new (square(unitCount), TempJob, UninitializedMemory);
            for (int i = 0; i < nativeCostMatrix.Length; i++)
            {
                int y = i / unitCount;
                int x = i - y * unitCount;
                float3 unitPosition = LinkedRegiment[y].Position;
                nativeCostMatrix[i] = distancesq(unitPosition, destinations[x]);
            }
            return nativeCostMatrix;
        }
    
        private void AssignIndexToUnits()
        {
            FormationData targetFormation = TargetFormation;
            using NativeArray<float> costMatrix = GetCostMatrix(targetFormation);
            using NativeArray<int> sortedIndex = JobifiedHungarianAlgorithm.FindAssignments(costMatrix, targetFormation.UnitCount);
            LinkedRegiment.ReorderElementsBySwap(sortedIndex);
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Move Logic ◈◈◈◈◈◈                                                                                       ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        private void UpdateProgressToTargetPosition()
        {
            if (reachTargetPosition) return;
            float distanceToTarget = distance(Position, LeaderTargetPosition);
            //Debug.Log($"Move.UpdateProgressToTargetPosition: distanceToTarget = {distanceToTarget}");
            reachTargetPosition = distanceToTarget <= REACH_DISTANCE_THRESHOLD;
        }
        
        //Rework : take into consideration pathfinding use of SimpleTerrain
        // Objectifs :
        // 1) follow basic path : need visual debug to see how the formation behave upon rotation and move (on regiment gizmos)
        // - Must see regiment forward (arrow) and units too
        
        // 2) Take obstacle into consideration
        // - reform the regiment to fit between obstacles then reform when possible
        
        private void CalculatePath()
        {
            currentPath = SimpleTerrain.Instance.GetCellPathTo(Position, LeaderTargetPosition, Temp).ToArray();
        }
        
        private Vector3 MoveRegiment()
        {
            if (reachTargetPosition) return Position; // Units may still be on their way
            
            float distanceMove = Time.deltaTime * CurrentSpeed;
            reachTargetPosition = distanceMove > distance(Position, LeaderTargetPosition);
            Vector3 translation = reachTargetPosition ? LeaderTargetPosition : Position + distanceMove * Position.direction(LeaderTargetPosition);
            return translation;
        }

        private Quaternion RotateRegiment()
        {
            return Quaternion.LookRotation(TargetFormation.DirectionForward, Vector3.up);
        }
    }
}