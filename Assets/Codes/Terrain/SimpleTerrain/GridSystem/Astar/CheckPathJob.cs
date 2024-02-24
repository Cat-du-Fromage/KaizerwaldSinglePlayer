using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using Kaizerwald.Utilities;
using UnityEngine.Serialization;

using static Kaizerwald.Utilities.KzwMath;
using static Kaizerwald.Utilities.InputSystemExtension;
using static Kaizerwald.Utilities.KzwGrid;

namespace Kaizerwald.TerrainBuilder
{
    [BurstCompile]
    public struct JCheckPath : IJob
    {
        [ReadOnly] public int GridWidth;
        [ReadOnly] public int StartIndex;
        [ReadOnly] public int EndIndex;
        
        [ReadOnly] public NativeArray<bool> ObstaclesGrid; // on aura pas forcément le format bool
        //il nous faudra les bool4 pour
        public NativeArray<Node> Nodes;

        [WriteOnly] public NativeReference<bool> PathExist;

        public void Execute()
        {
            NativeHashSet<int> openSet = new (16, Temp);
            NativeHashSet<int> closeSet = new (16, Temp);
            
            int startHCost = CalculateDistanceCost(Nodes[StartIndex], Nodes[EndIndex]);
            Nodes[StartIndex] = new Node(-1, 0, startHCost, Nodes[StartIndex].Coords);
            openSet.Add(StartIndex);
            
            int currentNode = GetLowestFCostNodeIndex(openSet);
            PathExist.Value = currentNode == EndIndex;
            
            NativeList<int> neighbors = new (4,Temp);
            while (!openSet.IsEmpty && !PathExist.Value) //notEmpty and noPath
            {
                openSet.Remove(currentNode);
                closeSet.Add(currentNode);
                GetNeighborCells(currentNode, neighbors, closeSet);
                if (neighbors.Length > 0)
                {
                    // foreach loop is expensive for NativeArray (https://www.jacksondunstan.com/articles/4713)
                    for (int i = 0; i < neighbors.Length; i++) 
                    {
                        openSet.Add(neighbors[i]);
                    }
                }
                neighbors.Clear();
                currentNode = GetLowestFCostNodeIndex(openSet);
                PathExist.Value = currentNode == EndIndex;
            };
        }

        private void GetNeighborCells(int index, NativeList<int> curNeighbors, NativeHashSet<int> closeSet)
        {
            int2 coords = GetXY2(index,GridWidth);
            for (int i = 0; i < 4; i++)
            {
                int neighborIndex = AdjacentCellFromIndex(index,i, coords, GridWidth);
                if (neighborIndex == -1 || ObstaclesGrid[neighborIndex] || closeSet.Contains(neighborIndex)) continue;
                
                Node neighborNode = Nodes[neighborIndex];
                int candidateCost = Nodes[index].GCost + CalculateDistanceCost(Nodes[index],neighborNode);
                if (candidateCost < neighborNode.GCost)
                {
                    curNeighbors.Add(neighborIndex);
                    int gCost = CalculateDistanceCost(neighborNode, Nodes[StartIndex]);
                    int hCost = CalculateDistanceCost(neighborNode, Nodes[EndIndex]);
                    Nodes[neighborIndex] = new Node(index, gCost, hCost, neighborNode.Coords);
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
    }
}
