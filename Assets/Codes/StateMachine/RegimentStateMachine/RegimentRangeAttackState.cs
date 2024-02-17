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
        private const float FOV_ANGLE = RegimentManager.RegimentFieldOfView;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private FormationData PreviousEnemyFormation;
        private Formation CurrentEnemyFormation;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private RegimentManager regimentManager => RegimentManager.Instance;
        public Regiment TargetEnemyRegiment { get; private set; }
        //public bool HasTarget { get; private set; }
        public bool HasTarget => LinkedRegiment.EnemyRegimentTargetData.EnemyTargetID != -1;
        private int AttackRange => RegimentType.Range;
        
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
            //TargetEnemyRegiment = rangeAttackOrder.TargetEnemyRegiment;
            if (!RegimentManager.Instance.TryGetRegiment(rangeAttackOrder.TargetEnemyRegiment, out Regiment target)) return;
            LinkedRegiment.EnemyRegimentTargetData.SetEnemyTarget(target);
            //HasTarget = rangeAttackOrder.TargetEnemyRegiment != null;
        }

        public override void OnEnter()
        {
            return;
        }

        public override void OnUpdate()
        {
            //Will check if enemyFormationChange (unit?)
            return;
        }

        public override void OnExit() { return; }

        public override EStates ShouldExit()
        {
            if (IdleExit() && !TryChangeTarget()) return EStates.Idle;
            
            //bool isEnemyInRange = StateExtension.CheckEnemiesAtRange(LinkedRegiment, AttackRange, out int targetID, FOV_ANGLE);
            //if (!isEnemyInRange) return ChaseExit() ? EStates.Move : EStates.Idle;
            
            return StateIdentity;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private bool TryChangeTarget()
        {
            bool hasOtherTargetInRange = StateExtension.CheckEnemiesAtRange(LinkedRegiment, AttackRange, out int targetID, FOV_ANGLE);
            if (hasOtherTargetInRange)
            {
                LinkedRegiment.EnemyRegimentTargetData.SetEnemyTarget(regimentManager.RegimentsByID[targetID]);
            }
            return hasOtherTargetInRange;
        }

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
