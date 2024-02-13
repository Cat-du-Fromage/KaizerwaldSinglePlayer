/*
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.Jobs;
using UnityEngine;

using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using Kaizerwald.FormationModule;
using Kaizerwald.Utilities;
using Unity.Mathematics;


namespace Kaizerwald
{
    public sealed class RegimentFormationMatrix : OrderedFormationBehaviour<Unit>
    {
        public static RegimentFormationMatrix AddAndInitialize(GameObject regiment, Formation formation, List<Unit> units)
        {
            RegimentFormationMatrix formationMatrix = (RegimentFormationMatrix)regiment.AddComponent<RegimentFormationMatrix>();
            formationMatrix.InitializeFormation(formation, units, regiment.transform.position);
            return formationMatrix;
        }
        
        //TODO: Find a way without Memory allocation (Unit[] tmpUnits = new Unit[Elements.Count])
        //Here was the issue
        //We receive sorted Indices [2,0,3,1], Which translate to:
        // Elements[2] = (tmp cache) Elements[0]
        // Elements[0] = (tmp cache) Elements[1]
        // Elements[3] = (tmp cache) Elements[2]
        // Elements[1] = (tmp cache) Elements[3]
        // BUT when Element[i] is assigned it still had IndexInFormation it has ([0,1,2,3] case before)
        // We need to assign IndexInFormation to the sorted Index [2,0,3,1] too!
        public void ReorderElements(NativeArray<int> indices)
        {
            if (indices.Length != Elements.Count) return;
            List<Unit> tmpUnits = new List<Unit>(Elements);
            for (int i = 0; i < Elements.Count; i++)
            {
                int sortedIndex = indices[i];
                Elements[sortedIndex] = tmpUnits[i];
                Elements[sortedIndex].SetIndexInFormation(sortedIndex);
            }
            ResetTransformsIndicators();
        }
        
        public void ReorderElements(NativeArray<int> distanceSortedIndices, NativeArray<int> sortedIndices)
        {
            Unit[] tmpUnits = new Unit[Elements.Count];
            Elements.CopyTo(tmpUnits);
            for (int i = 0; i < Elements.Count; i++)
            {
                int realIndex = distanceSortedIndices[i];
                int sortedIndex = sortedIndices[i];
                Elements[sortedIndex] = tmpUnits[realIndex];
                Elements[sortedIndex].SetIndexInFormation(sortedIndex);
            }
            ResetTransformsIndicators();
        }
        
        public void ReorderElementsBySwap(NativeArray<int> indices)
        {
            if (indices.Length != Elements.Count) return;
            //Find Cpp's Iota equivalent
            //NativeArray<int> indicesPosition = new (Enumerable.Range(0, Elements.Count).ToArray(), Temp);
            NativeArray<int> indicesPosition = new (Elements.Count, Temp, UninitializedMemory);
            for (int i = 0; i < Elements.Count; i++) indicesPosition[i] = i;
            
            for (int i = 0; i < Elements.Count; i++)
            {
                int sortedIndex = indices[i];
                int indexElementToSwapWith = indicesPosition.IndexOf(i);
                //Debug.Log($"sortedIndex = {sortedIndex} i = {i} position i = {indexElementToSwapWith} test indicesPosition[i] = {indicesPosition[i]}");
                (Elements[sortedIndex], Elements[indexElementToSwapWith]) = (Elements[indexElementToSwapWith], Elements[sortedIndex]);
                (indicesPosition[sortedIndex], indicesPosition[indexElementToSwapWith]) = (indicesPosition[indexElementToSwapWith], indicesPosition[sortedIndex]);
                Elements[sortedIndex].SetIndexInFormation(sortedIndex);
            }
            //for (int i = 0; i < Elements.Count; i++){Elements[i].SetIndexInFormation(i);}
            ResetTransformsIndicators();
        }
    }
}
*/
