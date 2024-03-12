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
            CombatStateBoard.TrySetEnemyTarget(rangeAttackOrder.TargetEnemyId, true);
        }

        public override void OnEnter() { return; }

        public override void OnUpdate()
        {
            CombatStateBoard.UpdateCachedFormation();
        }

        public override void OnExit() { return; }

        public override bool ShouldExit(out EStates nextState)
        {
            // RANGE -> Move
            nextState = GetExitState();
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

        private EStates GetExitState()
        {
            /*
            EStates nextState = EStates.Idle;
            if (CombatStateBoard.IsTargetValid())
            {
                if (IsTargetInRange())
                {
                    nextState = StateIdentity;
                }
                else if (CombatStateBoard.IsChasingTarget)
                {
                    nextState = EStates.Move;
                }
                else if (InputStateBoard.AutoFire && TryChangeTarget())
                {
                    nextState = StateIdentity;
                }
            }
            else //No Target => No Chase possible
            {
                if (InputStateBoard.AutoFire && TryChangeTarget())
                {
                    nextState = StateIdentity;
                }
            }
            return nextState;
            */
            
            if (!CombatStateBoard.IsTargetValid() && !InputStateBoard.AutoFire)
            {
                return EStates.Idle;
            }

            if (IsTargetInRange())
            {
                return StateIdentity;
            }

            if (CombatStateBoard.IsChasingTarget)
            {
                return EStates.Move;
            }

            if (!InputStateBoard.AutoFire || !TryChangeTarget())
            {
                return EStates.Idle;
            }
            
            return StateIdentity;
            
        }

        private bool IdleExit()
        {
            return !CombatStateBoard.IsTargetValid() || (!IsTargetInRange() && (!InputStateBoard.AutoFire || !TryChangeTarget()));
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
                CombatStateBoard.TrySetEnemyTarget(RegimentManager.Instance.RegimentsByID[targetID]);
            }
            else
            {
                CombatStateBoard.Clear();
            }
            return hasOtherTargetInRange;
        }
    }
}
