using System;
using System.Collections.Generic;
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
using Kaizerwald.FormationModule;
using Kaizerwald.TerrainBuilder;

using static Kaizerwald.Utilities.Core.KzwMath;
using static Kaizerwald.Utilities.Core.KzwGrid;
using static Kaizerwald.Utilities.Core.ArrayExtension;


namespace Kaizerwald.StateMachine
{
    // - Reforming Move
    // - Move
    public class RegimentMoveSequencerState : RegimentSequencerStateBase
    {
        private const float REACH_DISTANCE_THRESHOLD = 0.0125f;
        
        public Cell[] CurrentPath = Array.Empty<Cell>();
        
        public float3 LeaderTargetPosition => LinkedRegiment.TargetPosition;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        public RegimentMoveSequencerState(RegimentStateMachine stateMachine, List<StateBase<RegimentStateMachine>> sequence) 
            : base(stateMachine, EStates.Move, sequence)
        {
        }

        public override void OnSetup(Order order)
        {
            LinkedRegiment.SetDestination(order.TargetPosition, order.TargetFormation);
            CurrentPath = SimpleTerrain.Instance.GetCellPathTo(Position, LeaderTargetPosition).ToArray();
            
            base.OnSetup(order);
        }

        public override void OnUpdate()
        {
            
        }
    }
}
