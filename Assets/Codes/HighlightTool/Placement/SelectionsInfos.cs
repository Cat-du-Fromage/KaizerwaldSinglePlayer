using System;
using System.Collections.Generic;
using System.Linq;
using Kaizerwald.FormationModule;
using Unity.Collections;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;

using float2 = Unity.Mathematics.float2;

namespace Kaizerwald
{
    public class SelectionInfos
    {
        private readonly SelectionSystem selectionSystemAttached;
        
        public float2 MinMaxSelectionWidth { get; private set; }
        public int TotalUnitsSelected { get; private set; }
        public int[] SelectionsMinWidth { get; private set; }
        
        public List<HighlightRegiment> SelectedRegiments => selectionSystemAttached.SelectionRegister.ActiveHighlights;
        
        public SelectionInfos(SelectionSystem selectionSystem)
        {
            selectionSystemAttached = selectionSystem;
        }

        public void OnSelectionUpdate()
        {
            MinMaxSelectionWidth = SelectionInfoUtils.GetMinMaxSelectionWidth(SelectedRegiments);
            TotalUnitsSelected = SelectionInfoUtils.GetTotalUnitsSelected(SelectedRegiments);
            SelectionsMinWidth = SelectionInfoUtils.GetSelectionsMinWidth(SelectedRegiments).ToArray();
        }
    }
    
    public static class SelectionInfoUtils
    {
        public static float2 GetMinMaxSelectionWidth(List<HighlightRegiment> selectedRegiments)
        {
            float2 minMaxDistance = float2.zero;
            foreach (HighlightRegiment selection in selectedRegiments)
            {
                minMaxDistance += GetMinMaxFormationLength(selection.CurrentFormation);
            }
            return minMaxDistance;
        }
        
        public static int GetTotalUnitsSelected(List<HighlightRegiment> selectedRegiments)
        {
            return selectedRegiments.Sum(regiment => regiment.CurrentFormation.NumUnitsAlive);
        }

        private static float2 GetMinMaxFormationLength(in FormationData formation)
        {
            //return formation.DistanceUnitToUnit.x * (float2)formation.MinMaxRow;
            return formation.DistanceUnitToUnit * max(1, formation.MinMaxRow - 1);
        }
        
        public static NativeArray<int> GetSelectionsMinWidth(List<HighlightRegiment> selectedRegiments)
        {
            NativeArray<int> tmp = new (selectedRegiments.Count, Temp, UninitializedMemory);
            for (int i = 0; i < selectedRegiments.Count; i++)
            {
                tmp[i] = selectedRegiments[i].CurrentFormation.MinRow;
            }
            return tmp;
        }
        /*
        public static int[] GetSelectionsMinWidth(List<HighlightRegiment> selectedRegiments)
        {
            int[] tmp = new (selectedRegiments.Count, Temp, UninitializedMemory);
            for (int i = 0; i < selectedRegiments.Count; i++)
            {
                tmp[i] = selectedRegiments[i].CurrentFormation.MinRow;
            }
            return tmp;
        }
        */
    }
}