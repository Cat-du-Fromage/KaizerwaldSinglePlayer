using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    public class RegimentBlackboard
    {
        public bool IsRunning;
        
        public bool MeleeMode;
        public bool AutoFire;
        
        public int EnemyTarget;
        public bool HasTarget => EnemyTarget != 0;

        public RegimentBlackboard()
        {
            IsRunning = false;
            MeleeMode = false;
            AutoFire = true;
            EnemyTarget = 0;
        }
    }
}
