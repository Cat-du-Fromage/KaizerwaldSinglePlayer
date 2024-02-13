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
    public class Regiment_RangeAttackState : RegimentStateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private FormationData PreviousEnemyFormation;
        private Formation CurrentEnemyFormation;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Regiment TargetEnemyRegiment { get; private set; }
        public bool HasTarget { get; private set; }
        
        private int AttackRange => RegimentType.Range;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Regiment_RangeAttackState(RegimentBehaviourTree behaviourTree) : base(behaviourTree, EStates.Fire)
        {
        }
        
        public override bool ConditionEnter()
        {
            return base.ConditionEnter();
        }

        public override void OnSetup(Order order)
        {
            RangeAttackOrder rangeAttackOrder = (RangeAttackOrder)order;
            TargetEnemyRegiment = rangeAttackOrder.TargetEnemyRegiment;
            HasTarget = rangeAttackOrder.TargetEnemyRegiment != null;
            //RegimentBlackboard.OnOrder(order);
            //RegimentBlackboard.SetEnemyTarget(rangeAttackOrder.TargetEnemyRegiment);
            //RegimentBlackboard.SetChaseEnemyTarget(rangeAttackOrder.TargetEnemyRegiment, RegimentAttach.CurrentFormation);
            //Debug.Log($"Setup Fire State: {rangeAttackOrder.TargetEnemyRegiment.name}");
        }

        public override void OnEnter() { return; }

        public override void OnUpdate()
        {
            //Will check if enemyFormationChange
            return;
        }

        public override void OnExit() { return; }

        public override EStates ShouldExit()
        {
            if (IdleExit()) return EStates.Idle;
            //bool isEnemyInRange = StateExtension.IsTargetRegimentInRange(RegimentAttach, RegimentBlackboard.EnemyTarget, AttackRange);
            //if (!isEnemyInRange) return ChaseExit() ? EStates.Move : EStates.Idle;
            
            return StateIdentity;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private bool IdleExit()
        {
            return !HasTarget;
        }

        private bool HasEnemyFormationChange()
        {
            bool3 hasEnemyFormationChange = new bool3
            (
                PreviousEnemyFormation.NumUnitsAlive != CurrentEnemyFormation.NumUnitsAlive,
                PreviousEnemyFormation.WidthDepth != CurrentEnemyFormation.WidthDepth
            );
            return any(hasEnemyFormationChange);
        }
    }
}
