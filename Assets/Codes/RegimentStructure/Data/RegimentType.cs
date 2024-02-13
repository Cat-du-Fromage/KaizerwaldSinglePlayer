using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Kaizerwald.FormationModule;

namespace Kaizerwald
{
    [CreateAssetMenu(fileName = "NewRegimentType", menuName = "Regiment/RegimentType", order = 2)]
    public class RegimentType : ScriptableObject, IFormationInfo
    {
        [Header("Prefabs")]
        public RegimentClass RegimentClass;
        public GameObject UnitPrefab;
        public GameObject BulletPrefab;
        
        [Header("Stats")]
        public int Range = 0;
        public int Accuracy = 0;
        public int ReloadingSkill = 0;
        public int MarchSpeed = 1;
        public int RunSpeed = 2;
        public int Moral = 1;
        
        [Header("Optional")]
        public GameObject PlacementPrefabOverride;
        
        //Interface
        public int BaseNumUnits => RegimentClass.BaseNumberUnit;
        public int2 MinMaxRow => new (RegimentClass.MinRow, RegimentClass.MaxRow);
        public float2 UnitSize => new (RegimentClass.Category.UnitSize.x, RegimentClass.Category.UnitSize.z);
        public float SpaceBetweenUnit => RegimentClass.SpaceBetweenUnits;
        public float2 DistanceUnitToUnit => RegimentClass.DistanceUnitToUnit;
        
        public FormationData GetFormationData(float3 direction = default)
        {
            int2 minMaxRow = new (RegimentClass.MinRow, RegimentClass.MaxRow);
            float2 unitSize = new (RegimentClass.Category.UnitSize.x, RegimentClass.Category.UnitSize.z);
            return new FormationData(RegimentClass.BaseNumberUnit, minMaxRow, unitSize, RegimentClass.SpaceBetweenUnits, direction);
        }
        
        public Formation GetFormation(float3 direction)
        {
            return new Formation(BaseNumUnits, MinMaxRow, UnitSize, SpaceBetweenUnit, direction, RegimentClass.MinRow);
        }
    }
}
