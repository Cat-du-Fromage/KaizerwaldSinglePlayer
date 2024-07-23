#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

using static UnityEngine.Quaternion;
using static Unity.Mathematics.math;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using Kaizerwald.Utilities.Core;
using Kaizerwald.FormationModule;
using Kaizerwald.StateMachine;
using Kaizerwald.FieldOfView;
using Kaizerwald.Utilities.Core.Editor;

namespace Kaizerwald
{
    public sealed partial class Regiment : OrderedFormationBehaviour<Unit>, IOwnershipInformation
    {
        public bool DebugShowRegimentPosition = true;
        
        private void OnGUI()
        {
            int offset = 0;
            offset += FOVOnGUI(offset);
            offset += FindTargetOnGUI(offset);
        }
        
        private void OnDrawGizmos()
        {
            ShowRegiment();
            ShowUnitsAlive();
        }

        private void ShowRegiment()
        {
            if (!DebugShowRegimentPosition) return;
            Vector3 position = Position + up() + Forward;
            KzwGizmos.DrawWireCube(position, float3(0.5f,1.5f,0.5f), Color.magenta);
        }
    }
}
#endif