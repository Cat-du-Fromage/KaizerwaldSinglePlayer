using System;
using System.Collections.Generic;
using Kaizerwald.FormationModule;
using Kaizerwald.TerrainBuilder;
using Kaizerwald.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;
using static Unity.Mathematics.math;

using Kaizerwald.Utilities.Core;
using static Kaizerwald.Utilities.Core.KzwMath;
using static Kaizerwald.Utilities.Core.KzwGrid;
using static Kaizerwald.Utilities.Core.ArrayExtension;

namespace Kaizerwald.StateMachine
{
    public class RegimentPreMoveSubState : RegimentStateBase
    {
        
        public RegimentPreMoveSubState(RegimentStateMachine stateMachine) : base(stateMachine, EStates.Move)
        {
        }

        public override void OnSetup(Order order)
        {
            
        }

        public override void OnUpdate()
        {
            
        }

        public override bool ShouldExit(out EStates state)
        {
            state = StateIdentity;
            return false;
        }
    }
}
