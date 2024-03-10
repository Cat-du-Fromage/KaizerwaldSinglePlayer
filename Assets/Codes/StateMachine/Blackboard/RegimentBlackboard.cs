using System.Collections;
using System.Collections.Generic;
using Kaizerwald.FormationModule;
using Unity.Mathematics;
using UnityEngine;

namespace Kaizerwald
{
    public struct RegimentBlackboard
    {
        // ===== MotionStateBoard =====
        // FIXED DATA : contains in Nodes ?
        public float MinSpeed;
        public float MaxSpeed;
        
        // DYNAMIC DATA
        public bool IsRunning; //player toggle
        
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
    }
}
