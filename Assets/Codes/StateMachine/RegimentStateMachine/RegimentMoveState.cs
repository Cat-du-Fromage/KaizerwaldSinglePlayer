using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
using static Kaizerwald.Utilities.UnityMathematicsExtension;
using static Kaizerwald.Utilities.CSharpContainerUtils;

namespace Kaizerwald.StateMachine
{
    public sealed class RegimentMoveState : RegimentStateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private bool leaderReachTargetPosition = true;
        private float moveSpeed;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public EMoveType CurrentMoveType { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public bool IsAlreadyMoving => CurrentMoveType != EMoveType.None;
        public float3 LeaderTargetPosition => LinkedRegiment.TargetPosition;
        public float MarchSpeed => RegimentType.MarchSpeed;
        public float RunSpeed   => RegimentType.RunSpeed;
    
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Getters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public bool LeaderReachTargetPosition => leaderReachTargetPosition;
        public float MoveSpeed => moveSpeed;
        public bool IsRunning  => CurrentMoveType == EMoveType.Run;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Setters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void SetMoveSpeed(EMoveType moveType)
        {
            CurrentMoveType = moveType;
            moveSpeed = moveType == EMoveType.Run ? RunSpeed : MarchSpeed;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public RegimentMoveState(RegimentBehaviourTree behaviourTree) : base(behaviourTree,EStates.Move)
        {
        }

        public override bool ConditionEnter()
        {
            return base.ConditionEnter();
        }

        public override void OnSetup(Order order)
        {
            MoveOrder moveOrder = (MoveOrder)order;
            LinkedRegiment.SetDestination(moveOrder.LeaderTargetPosition, moveOrder.TargetFormation);
            AssignIndexToUnits();
            //AssignIndexToUnitsByRow();
            SetMoveSpeed(moveOrder.MoveType);
        }

        public override void OnEnter()
        {
            leaderReachTargetPosition = false;
            UpdateProgressToTargetPosition();
            CurrentFormation.SetFromFormation(TargetFormation);
        }

        public override void OnUpdate()
        {
            if (leaderReachTargetPosition) return;
            MoveRegiment();
        }

        public override void OnExit()
        {
            CurrentMoveType = EMoveType.None;
        }

        public override EStates ShouldExit()
        {
            UpdateProgressToTargetPosition();
            if (!leaderReachTargetPosition) return StateIdentity;
            return EStates.Idle;
            //return RegimentBlackboard.IsChasing ? EStates.Fire : EStates.Idle;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void UpdateProgressToTargetPosition()
        {
            if (leaderReachTargetPosition) return;
            leaderReachTargetPosition = distance(Position, LeaderTargetPosition) <= REACH_DISTANCE_THRESHOLD;
        }

        private void MoveRegiment()
        {
            if (leaderReachTargetPosition) return; // Units may still be on their way
            float3 translation = Time.deltaTime * moveSpeed * Position.DirectionTo(LeaderTargetPosition);
            
            BehaviourTree.CachedTransform.Translate(translation, Space.World);
            BehaviourTree.CachedTransform.LookAt(Position + TargetFormation.DirectionForward);
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ On Enter ◈◈◈◈◈◈                                                                                         ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
/*
        private void DebugMatrixCost(NativeArray<float> nativeCostMatrix, in FormationData formation)
        {
            StringBuilder stringBuilderDebug = new StringBuilder();
            stringBuilderDebug.Append($"DebugMatrixCost");
            for (int y = 0; y < formation.NumUnitsAlive; y++)
            {
                stringBuilderDebug.Append($"\n\rworker {y}\t|");
                for (int x = 0; x < formation.NumUnitsAlive; x++)
                {
                    int index = GetIndex(x, y, formation.NumUnitsAlive);
                    float value = ((int)(nativeCostMatrix[index] * 100)) / 100f;
                    stringBuilderDebug.Append($"{value}\t | ");
                }
            }
            Debug.Log(stringBuilderDebug);
        }
    */

        private NativeArray<float> GetCostMatrix(in FormationData targetFormation, Allocator returnedAllocator)
        {
            NativeArray<float3> destinations = targetFormation.GetUnitsPositionRelativeToRegiment(LeaderTargetPosition, Temp);
            NativeArray<float> nativeCostMatrix = new (square(targetFormation.NumUnitsAlive), returnedAllocator, UninitializedMemory);
            for (int i = 0; i < nativeCostMatrix.Length; i++)
            {
                (int x, int y) = GetXY(i, targetFormation.NumUnitsAlive);
                float3 unitPosition = LinkedRegiment[y].Position;
                //float distanceToLeaderDest = distance(unitPosition, BehaviourTree.Position);
                float distanceToUnitDestination = distancesq(unitPosition, destinations[x]);
                float distancePoint = distanceToUnitDestination;// + distanceToLeaderDest;
                nativeCostMatrix[i] = distancePoint;
            }
            //DebugMatrixCost(nativeCostMatrix, formation);
            return nativeCostMatrix;
        }
    
        private void AssignIndexToUnits()
        {
            FormationData targetFormation = TargetFormation;
            using NativeArray<float> costMatrix = GetCostMatrix(targetFormation, TempJob);
            using NativeArray<int> sortedIndex = JobifiedHungarianAlgorithm.FindAssignments(costMatrix, targetFormation.NumUnitsAlive);
            LinkedRegiment.ReorderElementsBySwap(sortedIndex);
        }
        

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                     ◆◆◆◆◆◆ PROTOTYPE(Chunked Algorithm) ◆◆◆◆◆◆                                     ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        // Tests
        public bool IsReversedOrder()
        {
            //normalizesafe( LeaderDestination.xz - Position.xz)
            
            bool isBehind = dot(Position.DirectionTo(LeaderTargetPosition).xz, CurrentFormation.Direction2DForward )<= -0.5f;
            bool lookOpposite = dot(TargetFormation.Direction2DForward, CurrentFormation.Direction2DForward) <= -0.95f;
            Debug.Log($"IsReversedOrder isBehind = {isBehind}, lookOpposite = {lookOpposite}");
            return isBehind && !lookOpposite;
        }


        private NativeArray<float> GetCostSliceMatrix(NativeSlice<int> closestIndices, NativeSlice<float3> destinations)
        {
            NativeArray<float> nativeCostMatrix = new (square(destinations.Length), TempJob, UninitializedMemory);
            for (int i = 0; i < nativeCostMatrix.Length; i++)
            {
                (int x, int y) = GetXY(i, destinations.Length);
                int unitIndex = closestIndices[y];
                float3 unitPosition = LinkedRegiment[unitIndex].Position;
                float distanceToUnitDestination = distancesq(unitPosition, destinations[x]);
                nativeCostMatrix[i] = distanceToUnitDestination;
            }
            return nativeCostMatrix;
        }
        
        private NativeArray<float> GetWholeCostMatrix(NativeArray<int> closestIndices, float3 targetPosition, in FormationData formation)
        {
            (int depth, int width) = (formation.Depth, formation.Width);
            int realLength = square(formation.Width) * formation.Depth;
            NativeArray<float> nativeCostMatrix = new (realLength, TempJob, UninitializedMemory);
            NativeArray<float3> destinations = formation.GetUnitsPositionRelativeToRegiment(targetPosition, Temp);
            //destinations.Reverse();
            
            for (int i = 0; i < depth; i++)
            {
                bool isLastLine = i == depth - 1;
                (int startSliceIndex, int sliceLength) = (i * width, !isLastLine ? width : formation.NumUnitsLastLine);
                
                NativeSlice<float3> destinationSlice = destinations.Slice(startSliceIndex, sliceLength);
                NativeSlice<int> sortedIndicesSlice = closestIndices.Slice(startSliceIndex, sliceLength);
                
                //CAREFULL! not the same Length
                int costSliceStart = i * square(formation.Width);
                int costSliceLength = square(sliceLength);
                NativeSlice<float> sliceCost = nativeCostMatrix.Slice(costSliceStart, costSliceLength);
                
                //Fill nativeCostMatrix ordered by Rows
                for (int sliceIndex = 0; sliceIndex < sliceCost.Length; sliceIndex++)
                {
                    (int x, int y) = GetXY(sliceIndex, sliceLength);
                    
                    int unitIndex = sortedIndicesSlice[y];
                    float3 unitPosition = LinkedRegiment[unitIndex].Position;
                    
                    float distanceToUnitDestination = distancesq(unitPosition, destinationSlice[x]);
                    sliceCost[sliceIndex] = distanceToUnitDestination;
                }
            }
            return nativeCostMatrix;
        }

        private NativeArray<int> GetSortedIndicesByRawDistance(float3 targetPosition, in FormationData formation)
        {
            NativeParallelHashMap<float, int> buffer = new (formation.NumUnitsAlive, Temp);
            NativeArray<float> distances = new (formation.NumUnitsAlive, Temp, UninitializedMemory);
            for (int i = 0; i < formation.NumUnitsAlive; i++)
            {
                float totalDistance = distancesq(LinkedRegiment[i].Position, targetPosition);
                distances[i] = totalDistance;
                buffer.Add(totalDistance, i);
            }
            distances.Sort();
            //bool isReversed = IsReversedOrder();
            //bool lookOpposite = dot(TargetFormation.Direction2DForward, CurrentFormation.Direction2DForward) <= -0.95f;
            //bool nearCross = dot(TargetFormation.Direction2DForward, CurrentFormation.Direction2DForward) <= 0.05f;;
            //TODO: check if NOT behind and direction eventually intersect
            //if (isReversed || lookOpposite || nearCross) { distances.Reverse(); }
            
            //if we reverse here we got the problem for "forward movement" (inverse of the current issue)
            NativeArray<int> tmpSortedIndices = new (formation.NumUnitsAlive, TempJob, UninitializedMemory);
            for (int i = 0; i < tmpSortedIndices.Length; i++) tmpSortedIndices[i] = buffer[distances[i]];
            return tmpSortedIndices;
        }
        
        private void AssignIndexToUnitsByRow()
        {
            FormationData targetFormation = LinkedRegiment.TargetFormation;
            (int depth, int width) = (targetFormation.Depth, targetFormation.Width);
            
            using NativeArray<int> closestIndices = GetSortedIndicesByRawDistance(LeaderTargetPosition, targetFormation);
            // ========================================================================================================
            //Calcul par ligne/row
            using NativeArray<int> agentTasks = new (square(targetFormation.NumUnitsAlive), TempJob, UninitializedMemory);
            using NativeArray<float> costMatrix = GetWholeCostMatrix(closestIndices, LeaderTargetPosition, targetFormation);
            
            NativeArray<JobHandle> jobHandles = new (depth, Temp, UninitializedMemory);
            for (int i = 0; i < depth; i++)
            {
                bool isLastLine = i == depth - 1;
                (int startSliceIndex, int sliceLength) = (i * width, !isLastLine ? width : targetFormation.NumUnitsLastLine);
                
                NativeSlice<int> agentTaskSlice = agentTasks.Slice(startSliceIndex, sliceLength);
                NativeSlice<float> costAtRow = costMatrix.Slice(i * square(width), square(sliceLength));
                jobHandles[i] = JobifiedHungarianAlgorithm.FindAssignments(agentTaskSlice, costAtRow, sliceLength, startSliceIndex);
            }
            JobHandle.CompleteAll(jobHandles);
            // tmpSortedIndices -> agentTasks
            LinkedRegiment.ReorderElements(closestIndices, agentTasks);
        }
        
        
    }
}

/*
 private void AssignIndexToUnitsByRow2()
        {
            FormationData targetFormation = FormationMatrix.TargetFormation;
            using NativeArray<int> closestIndices = GetSortedIndicesByRawDistance(LeaderDestination, targetFormation);
            // ========================================================================================================
            //Calcul par ligne/row
            using NativeArray<int> agentTasks = new (square(targetFormation.NumUnitsAlive), TempJob, UninitializedMemory);
            using NativeArray<float> costMatrix = GetWholeCostMatrix2(closestIndices, LeaderDestination, targetFormation, IsReversedOrder());
            
            NativeArray<JobHandle> jobHandles = new (targetFormation.Depth, Temp, UninitializedMemory);
            for (int i = 0; i < targetFormation.Depth; i++)
            {
                int startSliceIndex = i * targetFormation.Width;
                int sliceLength = i == targetFormation.Depth - 1 ? targetFormation.NumUnitsLastLine : targetFormation.Width;
                NativeSlice<int> agentTaskSlice = agentTasks.Slice(startSliceIndex, sliceLength);
                NativeSlice<float> costAtRow = costMatrix.Slice(i * square(targetFormation.Width), square(sliceLength));
                jobHandles[i] = JobifiedHungarianAlgorithm.FindAssignments(agentTaskSlice, costAtRow, sliceLength, startSliceIndex);
            }
            JobHandle.CompleteAll(jobHandles);
            // tmpSortedIndices -> agentTasks
            FormationMatrix.ReorderElements(closestIndices, agentTasks);
        }
        
        private NativeArray<float> GetWholeCostMatrix2(NativeArray<int> closestIndices, float3 targetPosition, in FormationData formation, bool isReversed)
        {
            int realLength = square(formation.Width) * formation.Depth;
            NativeArray<float> nativeCostMatrix = new (realLength, TempJob, UninitializedMemory);
            NativeArray<float3> destinations = formation.GetUnitsPositionRelativeToRegiment(targetPosition, Temp);
            
            for (int i = 0; i < formation.Depth; i++)
            {
                int startSliceIndex = i * formation.Width;
                int sliceLength = i == formation.Depth - 1 ? formation.NumUnitsLastLine : formation.Width;
                NativeSlice<float3> destinationSlice = GetDestinationSlice(i, formation);
                //NativeSlice<float3> destinationSlice = destinations.Slice(startSliceIndex, sliceLength);
                NativeSlice<int> sortedIndicesSlice = closestIndices.Slice(startSliceIndex, sliceLength);
                
                //CAREFULL! not the same Length
                int costSliceStart = i * square(formation.Width);
                int costSliceLength = square(sliceLength);
                NativeSlice<float> sliceCost = nativeCostMatrix.Slice(costSliceStart, costSliceLength);
                
                //Fill nativeCostMatrix ordered by Rows
                for (int sliceIndex = 0; sliceIndex < sliceCost.Length; sliceIndex++)
                {
                    (int x, int y) = GetXY(sliceIndex, sliceLength);
                    
                    int unitIndex = sortedIndicesSlice[y];
                    float3 unitPosition = FormationMatrix[unitIndex].Position;
                    
                    float distanceToUnitDestination = distancesq(unitPosition, destinationSlice[x]);
                    sliceCost[sliceIndex] = distanceToUnitDestination;
                }
            }
            return nativeCostMatrix;

            NativeSlice<float3> GetDestinationSlice(int index, in FormationData formation)
            {
                int startSliceIndex = index * formation.Width;
                if (isReversed)
                {
                    startSliceIndex = formation.NumUnitsAlive - 1;
                    int lastLineRemoved = index >= 0 ? formation.NumUnitsLastLine : 0;
                    startSliceIndex -= lastLineRemoved;
                    int widthToRemove = index > 0 ? max(0,index-1) * formation.Width : 0;
                    startSliceIndex -= widthToRemove;
                }
                
                //int startSliceIndex = index * formation.Width;
                int sliceLength = index == formation.Depth - 1 ? formation.NumUnitsLastLine : formation.Width;
                if (isReversed)
                {
                    sliceLength = index == 0 ? formation.NumUnitsLastLine : formation.Width;
                }
                Debug.Log($"for index = {index}: startSliceIndex = {startSliceIndex}, sliceLength = {sliceLength}");
                NativeSlice<float3> destinationSlice = destinations.Slice(startSliceIndex, sliceLength);
                return destinationSlice;
            }
        }
 */
