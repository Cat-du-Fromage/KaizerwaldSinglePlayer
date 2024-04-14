using System.Collections;
using System.Collections.Generic;
using Kaizerwald.FormationModule;
using Kaizerwald.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace Kaizerwald
{
    public struct AbilityBoard
    {
        public bool AutoFire; //player toggle
        public bool IsRunning; //player toggle
        public bool IsInMeleeMode; //player toggle
    }

    public struct DynamicStatusBoard
    {
        public bool HasTarget;
        public bool IsMoving; // ReachDestination
        public bool IsChasing;
        
        public int TargetEnemyId;
        public float3 TargetPosition;
        public FormationData TargetFormationData;
    }
    
    public class RegimentBlackboard
    {
        public AbilityBoard AbilityBoard;
        public DynamicStatusBoard DynamicStatusBoard;
    }
}
