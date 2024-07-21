using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Kaizerwald
{
    [CreateAssetMenu(fileName = "NewRegimentClass", menuName = "Regiment/Class", order = 1)]
    public class RegimentClass : ScriptableObject
    {
        [Header("Category Prefabs")]
        public RegimentCategory Category;

        [Header("Stats")]
        public int BaseNumberUnit = 20;
        public int MinRow = 4;
        public int MaxRow = 10;
        public float SpaceBetweenUnits = 0.5f;

        public float FovSideAngleDegrees = 60;

        public float2 DistanceUnitToUnit => SpaceBetweenUnits + ((float3)Category.UnitSize).xz;
    }
}
