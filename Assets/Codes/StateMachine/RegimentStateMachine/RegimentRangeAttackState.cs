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
using Unity.VisualScripting;
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
        private Regiment CurrentEnemyTarget => CombatStateBoard.EnemyTarget;
        public bool HasTarget => CombatStateBoard.EnemyTargetID != -1;
        
        // RegimentType
        public int MaxRange => RegimentType.Range;
        public int Accuracy => RegimentType.Accuracy;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public RegimentRangeAttackState(RegimentBehaviourTree behaviourTree) : base(behaviourTree, EStates.Fire)
        {
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        // Order to attack a specific Units
        public override void OnSetup(Order order)
        {
            RangeAttackOrder rangeAttackOrder = (RangeAttackOrder)order;
            if (!RegimentManager.Instance.RegimentExist(rangeAttackOrder.TargetEnemyId)) return;
            CombatStateBoard.SetEnemyTarget(rangeAttackOrder.TargetEnemyId, true);
        }

        public override void OnEnter() { return; }

        public override void OnUpdate()
        {
            CombatStateBoard.UpdateCachedFormation();
        }

        public override void OnExit() { return; }

        public override bool ShouldExit(out EStates nextState)
        {
            Exit(out nextState);
            /*
            if (IdleExit() && !CombatStateBoard.IsChasingTarget)
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
            */
            return nextState != StateIdentity;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void Exit(out EStates nextState)
        {
            if (!CombatStateBoard.IsTargetValid())
            {
                nextState = EStates.Idle;
            }
            else if (IsTargetInRange())
            {
                nextState = StateIdentity;
            }
            else if (CombatStateBoard.IsChasingTarget)
            {
                nextState = EStates.Move;
            }
            else if (!TryChangeTarget())
            {
                nextState = EStates.Idle;
            }
            else
            {
                nextState = StateIdentity;
            }
        }

        private bool IdleExit()
        {
            return !HasTarget || (!IsTargetInRange() && !TryChangeTarget());
        }

        private bool MoveExit()
        {
            bool chaseEnemy = CombatStateBoard.IsChasingTarget;
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
                CombatStateBoard.SetEnemyTarget(RegimentManager.Instance.RegimentsByID[targetID]);
            }
            else
            {
                CombatStateBoard.Clear();
            }
            return hasOtherTargetInRange;
        }
    }
}
