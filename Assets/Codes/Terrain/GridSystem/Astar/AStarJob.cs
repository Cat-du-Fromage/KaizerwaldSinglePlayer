using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using Kaizerwald.Utilities;

using static Kaizerwald.Utilities.Core.KzwMath;
using static Kaizerwald.Utilities.Core.InputSystemExtension;
using static Kaizerwald.Utilities.Core.KzwGrid;

namespace Kaizerwald.TerrainBuilder
{
    [BurstCompile]
    public struct JAStar : IJob
    {
        [ReadOnly] public int2 GridSizeXY;
        [ReadOnly] private int NumNeighbors;
        
        [ReadOnly] public int StartNodeIndex;
        [ReadOnly] public int EndNodeIndex;
        
        [ReadOnly] public NativeArray<bool> ObstaclesGrid; 
        [WriteOnly] public NativeList<int> PathList; // if PathNode.Length == 0 means No Path!
        
        private NativeArray<Node> Nodes;
        
        public JAStar(NativeList<int> pathList, int2 gridSizeXY, int startIndex, int endIndex, NativeArray<Node> nodes, NativeArray<bool> obstaclesGrid, bool allowDiagonal = false)
        {
            GridSizeXY = gridSizeXY;
            StartNodeIndex = startIndex;
            EndNodeIndex = endIndex;
            ObstaclesGrid = obstaclesGrid;
            PathList = pathList;
            //CAREFULL BUG SOMETIMES SONT KNOW WHY? unreliable?
            Nodes = nodes;
            NumNeighbors = allowDiagonal ? 8 : 4;
        }
        
        public void Execute()
        {
            //cant be moved to private field because "[DeallocateOnJobCompletion]" is not available for NativeHashSet..
            NativeHashSet<int> openSet = new (16, Temp);
            NativeHashSet<int> closeSet = new (16, Temp);
            
            int startHCost = CalculateDistanceCost(Nodes[StartNodeIndex], Nodes[EndNodeIndex]);
            Nodes[StartNodeIndex] = new Node(-1, 0, startHCost, Nodes[StartNodeIndex].Coords);
            openSet.Add(StartNodeIndex);
            
            NativeList<int> neighbors = new (NumNeighbors,Temp);
            while (!openSet.IsEmpty)
            {
                int currentNode = GetLowestFCostNodeIndex(openSet);
                if (currentNode == EndNodeIndex) //Check if we already arrived
                {
                    CalculatePath();
                    return;
                }
                
                //Add "already check" Node AND remove from "To check"
                openSet.Remove(currentNode);
                closeSet.Add(currentNode);
                
                GetNeighborCells(currentNode, neighbors, closeSet);// Add Neighbors to OpenSet
                if (neighbors.Length > 0)
                {
                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        openSet.Add(neighbors[i]);
                    }
                }
                neighbors.Clear();
            }
        }

        private void CalculatePath()
        {
            PathList.Add(EndNodeIndex);
            int nodeIndex = EndNodeIndex;
            while(nodeIndex != StartNodeIndex)
            {
                nodeIndex = Nodes[nodeIndex].CameFromNodeIndex;
                PathList.Add(nodeIndex);
            }
        }
        
        private void GetNeighborCells(int index, NativeList<int> curNeighbors, NativeHashSet<int> closeSet)
        {
            int2 coords = GetXY2(index,GridSizeXY.x);
            for (int i = 0; i < NumNeighbors; i++)
            {
                int neighborId = AdjacentCellFromIndex(index,i, coords, GridSizeXY);
                if (neighborId == -1 || ObstaclesGrid[neighborId] || closeSet.Contains(neighborId)) continue;
                
                int candidateCost = Nodes[index].GCost + CalculateDistanceCost(Nodes[index],Nodes[neighborId]);
                if (candidateCost < Nodes[neighborId].GCost)
                {
                    curNeighbors.Add(neighborId);
                    int gCost = CalculateDistanceCost(Nodes[neighborId], Nodes[StartNodeIndex]);
                    int hCost = CalculateDistanceCost(Nodes[neighborId], Nodes[EndNodeIndex]);
                    Nodes[neighborId] = new Node(index, gCost, hCost, Nodes[neighborId].Coords);
                }
            }
        }

        private int GetLowestFCostNodeIndex(NativeHashSet<int> openSet)
        {
            int lowest = -1;
            foreach (int index in openSet)
            {
                lowest = lowest == -1 ? index : lowest;
                lowest = select(lowest, index, Nodes[index].FCost < Nodes[lowest].FCost);
            }
            return lowest;
        }
        
        private int CalculateDistanceCost(in Node a, in Node b)
        {
            int2 xyDistance = abs(a.Coords - b.Coords);
            int remaining = abs(xyDistance.x - xyDistance.y);
            return 14 * cmin(xyDistance) + 10 * remaining;
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Process ◈◈◈◈◈◈                                                                                          ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public static JobHandle Schedule(NativeList<int> pathList, int2 gridSizeXY, int start, int end, NativeArray<Node> nodes, NativeArray<bool> obstaclesGrid, bool allowDiagonal = false, JobHandle dependency = default)
        {
            NativeArray<Node> tmpNodes = new NativeArray<Node>(nodes, TempJob);
            JAStar job = new JAStar(pathList, gridSizeXY, start, end, tmpNodes, obstaclesGrid, allowDiagonal);
            JobHandle jh1 = job.Schedule(dependency);
            tmpNodes.Dispose(jh1);
            JobHandle jh2 = JReversePath.Process(pathList, jh1);
            return jh2;
        }
        
        public static JobHandle Schedule(NativeList<int> pathList, int2 gridSizeXY, int start, int end, NativeArray<Node> nodes,  bool allowDiagonal = false, JobHandle dependency = default)
        {
            NativeArray<bool> obstaclesGrid = new (nodes.Length, TempJob, ClearMemory);
            JobHandle jh = Schedule(pathList, gridSizeXY, start, end, nodes, obstaclesGrid, allowDiagonal, dependency);
            obstaclesGrid.Dispose(jh);
            return jh;
        }
    }
    
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ REVERSE ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    [BurstCompile]
    public struct JReversePath : IJob
    {
        public NativeList<int> PathList;
        public void Execute()
        {
            int start = 0;
            int end = PathList.Length - 1;
            while (start < end)
            {
                (PathList[start], PathList[end]) = (PathList[end], PathList[start]);
                start++;
                end--;
            }
        }

        public static JobHandle Process(NativeList<int> pathList, JobHandle dependency = default)
        {
            JReversePath job = new JReversePath() { PathList = pathList };
            JobHandle jh = job.Schedule(dependency);
            return jh;
        }
    }
}
