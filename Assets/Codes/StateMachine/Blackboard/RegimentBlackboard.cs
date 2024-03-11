using System.Collections;
using System.Collections.Generic;
using Kaizerwald.FormationModule;
using Kaizerwald.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace Kaizerwald
{
    public class RegimentBlackboard
    {
        public const float REACH_DISTANCE_THRESHOLD = 0.0125f;
        
        // ===== MotionStateBoard =====
        // FIXED DATA : contains in Nodes ?
        public float MinSpeed;
        public float MaxSpeed;
        
        // DYNAMIC DATA
        public bool IsRunning; //player toggle

        public float3 TargetPosition;
        public bool TargetPositionReach;
        public int TargetFormationWidth;
        
        // calcul made on Node ?
        //public float CurrentSpeed => IsRunning ? MaxSpeed : MinSpeed;
        
        // ===== COMBAT =====
        // FIXED DATA : contains in Nodes ? (Soucis: move a besoin de accuracy!)
        public float Range;
        public float Accuracy;
        
        // DYNAMIC DATA
        public bool AutoFire; //player toggle
        public bool IsInMeleeMode; //player toggle
        
        public bool IsChasing;
        public int TargetEnemyId;
        public Regiment TargetEnemy;

        public void UpdateBoard(float3 currentPosition)
        {
            float reachThreshold = IsChasing && !IsInMeleeMode ? Range : REACH_DISTANCE_THRESHOLD;
            TargetPositionReach = math.distance(currentPosition.xz, TargetPosition.xz) < reachThreshold;
        }
    }
}
