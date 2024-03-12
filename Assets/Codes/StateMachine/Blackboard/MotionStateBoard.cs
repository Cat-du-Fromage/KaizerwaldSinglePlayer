using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Kaizerwald.Utilities;

namespace Kaizerwald.StateMachine
{
    public class MotionStateBoard
    {
        public bool TargetPositionReach { get; private set; }
        public float3 TargetPosition { get; private set; }

        public int TargetFormationWidth;

        public void SetTargetPosition(float3 targetPosition)
        {
            if (targetPosition.IsAlmostEqual(TargetPosition)) return;
            TargetPositionReach = false;
        }
    }
}
