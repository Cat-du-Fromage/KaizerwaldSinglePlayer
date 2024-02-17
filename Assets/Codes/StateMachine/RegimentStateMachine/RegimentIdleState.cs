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

using static Kaizerwald.Utilities.KzwMath;
using static Kaizerwald.Utilities.UnityMathematicsExtension;
using static Kaizerwald.Utilities.CSharpContainerUtils;

namespace Kaizerwald.StateMachine
{
    public sealed class RegimentIdleState : RegimentStateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public bool AutoFire { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private int AttackRange => BehaviourTree.RegimentType.Range;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Setters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public void AutoFireOn()  => AutoFire = true;
        public void AutoFireOff() => AutoFire = false;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public RegimentIdleState(RegimentBehaviourTree behaviourTree) : base(behaviourTree, EStates.Idle)
        {
        }

        //Maybe "Stop" button like in Total war?
        public override void OnSetup(Order order) { return; }

        public override void OnEnter() { return; }

        public override void OnUpdate() { return; }

        public override void OnExit() { return; }

        public override EStates ShouldExit()
        {
            if (FireExit()) return EStates.Fire;
            
            return StateIdentity;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private bool FireExit()
        {
            const float fovAngle = RegimentManager.RegimentFieldOfView;
            
            bool enemyInRange = StateExtension.CheckEnemiesAtRange(LinkedRegiment, AttackRange, out int targetID, fovAngle);
            if (enemyInRange) LinkedRegiment.EnemyRegimentTargetData.SetEnemyTarget(targetID);
            
            return enemyInRange;
        }
        
        private bool MoveExit()
        {
            return false;
        }
    }
}
