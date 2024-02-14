using System;
using System.Collections.Generic;
using System.Linq;
using Kaizerwald.FormationModule;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
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
        
        public List<HighlightRegiment> SelectedRegiments => selectionSystemAttached.SelectionRegister.ActiveHighlights;
        
        public SelectionInfos(SelectionSystem selectionSystem)
        {
            selectionSystemAttached = selectionSystem;
        }

        public void OnSelectionUpdate()
        {
            MinMaxSelectionWidth = SelectionUtils.GetMinMaxSelectionWidth(SelectedRegiments);
            TotalUnitsSelected = SelectionUtils.GetTotalUnitsSelected(SelectedRegiments);
        }
    }
    
    public static class SelectionUtils
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
            //CORRECT Because we use FormationData!!!!
            return formation.DistanceUnitToUnit * max(1, formation.MinMaxRow - 1);
        }
        
        public static NativeArray<int> GetSelectionsMinWidth(List<HighlightRegiment> selectedRegiments)
        {
            NativeArray<int> tmp = new (selectedRegiments.Count, Temp, UninitializedMemory);
            for (int i = 0; i < selectedRegiments.Count; i++)
            {
                //Contrary To Formation Data Formation MinRow is Fixe and does not take in account NumUnitsAlive
                FormationData formationData = selectedRegiments[i].CurrentFormation;
                tmp[i] = formationData.MinRow;
                //tmp[i] = min(selectedRegiments[i].CurrentFormation.MinRow, selectedRegiments[i].CurrentFormation.NumUnitsAlive);
            }
            return tmp;
        }
    }
}