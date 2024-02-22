using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using float2 = Unity.Mathematics.float2;

using Kaizerwald.FormationModule;
using static Kaizerwald.Utilities.KzwMath;

namespace Kaizerwald.StateMachine
{
    public class RegimentRangeAttackState : RegimentStateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private FormationData PreviousEnemyFormation;
        private Formation CurrentEnemyFormation;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        // EnemyRegimentTargetData
        private Regiment CurrentEnemyTarget => EnemyRegimentTargetData.EnemyTarget;
        public bool HasTarget => EnemyRegimentTargetData.EnemyTargetID != -1;
        
        // RegimentType
        public int MaxRange => RegimentType.Range;
        public int Accuracy => RegimentType.Accuracy;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public RegimentRangeAttackState(RegimentBehaviourTree behaviourTree) : base(behaviourTree, EStates.Fire)
        {
        }
        
        public override bool ConditionEnter()
        {
            return base.ConditionEnter();
        }

        public override void OnSetup(Order order)
        {
            RangeAttackOrder rangeAttackOrder = (RangeAttackOrder)order;
            if (!RegimentManager.Instance.TryGetRegiment(rangeAttackOrder.TargetEnemyRegiment, out Regiment target)) return;
            EnemyRegimentTargetData.SetEnemyTarget(target);
        }

        public override void OnEnter()
        {
            return;
        }

        public override void OnUpdate()
        {
            EnemyRegimentTargetData.UpdateCachedFormation();
        }

        public override void OnExit() { return; }

        public override EStates ShouldExit()
        {
            if (IdleExit())
            {
                return EStates.Idle;
            }
            return StateIdentity;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private bool IsTargetInRange()
        {
            return StateExtension.IsTargetRegimentInRange(LinkedRegiment, CurrentEnemyTarget, MaxRange, FOV_ANGLE);
        }
        
        private bool TryChangeTarget()
        {
            bool hasOtherTargetInRange = StateExtension.CheckEnemiesAtRange(LinkedRegiment, MaxRange, out int targetID, FOV_ANGLE);
            if (hasOtherTargetInRange)
            {
                EnemyRegimentTargetData.SetEnemyTarget(RegimentManager.Instance.RegimentsByID[targetID]);
            }
            else
            {
                EnemyRegimentTargetData.Clear();
            }
            return hasOtherTargetInRange;
        }

        private bool IdleExit()
        {
            return (!HasTarget || !IsTargetInRange()) && !TryChangeTarget();
        }
    }
}
