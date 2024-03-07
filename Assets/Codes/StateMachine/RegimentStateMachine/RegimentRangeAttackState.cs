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
    public sealed class RegimentRangeAttackState : RegimentStateBase
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

        public override void OnSetup(Order order)
        {
            RangeAttackOrder rangeAttackOrder = (RangeAttackOrder)order;
            if (!RegimentManager.Instance.RegimentExist(rangeAttackOrder.TargetEnemyId)) return;
            EnemyRegimentTargetData.SetEnemyTarget(rangeAttackOrder.TargetEnemyId);
        }

        public override void OnEnter() { return; }

        public override void OnUpdate()
        {
            EnemyRegimentTargetData.UpdateCachedFormation();
        }

        public override void OnExit() { return; }

        public override bool ShouldExit(out EStates nextState)
        {
            /*
            if (EnemyRegimentTargetData.IsTargetValid() && !IsTargetInRange())
            {
                if (EnemyRegimentTargetData.IsTargetLocked)
                {
                    nextState = EStates.Move;
                }
                else if(!TryChangeTarget())
                {
                    nextState = EStates.Idle;
                }
                else
                {
                    nextState = StateIdentity;
                }
            }
            else
            {
                nextState = StateIdentity;
            }
            */
            if (IdleExit() && !EnemyRegimentTargetData.IsTargetLocked)
            {
                nextState = EStates.Idle;
            }
            else if (MoveExit())
            {
                nextState = EStates.Move;
            }
            else
            {
                nextState = StateIdentity;
            }
            
            return nextState != StateIdentity;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private bool IdleExit()
        {
            return !HasTarget || (!IsTargetInRange() && !TryChangeTarget());
        }

        private bool MoveExit()
        {
            bool chaseEnemy = EnemyRegimentTargetData.IsTargetLocked;
            return false;
            //return chaseEnemy && EnemyRegimentTargetData.EnemyTarget;
        }

        private bool IsTargetInRange()
        {
            return StateExtension2.IsTargetRegimentInRange(LinkedRegiment, CurrentEnemyTarget, MaxRange, FOV_ANGLE);
        }
        
        private bool TryChangeTarget()
        {
            bool hasOtherTargetInRange = StateExtension2.CheckEnemiesAtRange(LinkedRegiment, MaxRange, out int targetID, FOV_ANGLE);
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
    }
}
