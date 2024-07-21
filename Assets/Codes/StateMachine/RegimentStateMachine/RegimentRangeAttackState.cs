using System;
using System.Collections;
using System.Collections.Generic;
using Kaizerwald.FieldOfView;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine.Jobs;
using Kaizerwald.Utilities;

using static Unity.Mathematics.math;
using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using static Kaizerwald.Utilities.Core.KzwMath;
using static Kaizerwald.Utilities.Core.KzwGeometry;
using float2 = Unity.Mathematics.float2;

using Kaizerwald.FormationModule;

namespace Kaizerwald.StateMachine
{
    public sealed class RegimentRangeAttackState : RegimentStateBase
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public readonly int MaxRange;
        public readonly int Accuracy;

        private FormationData PreviousEnemyFormation;
        private Formation CurrentEnemyFormation;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public RegimentBlackboard Blackboard => StateMachine.RegimentBlackboard;
        public FieldOfViewController FieldOfView => LinkedRegiment.FieldOfViewController;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public RegimentRangeAttackState(RegimentStateMachine stateMachine) : base(stateMachine, EStates.Fire)
        {
            MaxRange = stateMachine.RegimentType.Range;
            Accuracy = stateMachine.RegimentType.Accuracy;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ STATE METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public override bool ConditionEnter()
        {
            // Passif : 
            // - MeleeMode == false
            // - AutoFire == true
            // - CheckEnemiesAtRange() == true
            if (Blackboard.MeleeMode || !Blackboard.AutoFire)
            {
                return false;
            }
            else
            {
                return CheckEnemiesAtRange(out Blackboard.EnemyTarget);
            }
        }

        // Order to attack a specific Units
        public override void OnSetup(Order order)
        {
            return;
        }

        public override void OnEnter()
        {
            return;
        }

        public override void OnUpdate()
        {
            return;
        }

        public override void OnExit()
        {
            return;
        }

        public override bool ShouldExit(out EStates nextState)
        {
            // RANGE -> Move
            nextState = GetExitState();
            return nextState != StateIdentity;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private EStates GetExitState()
        {
            bool targetExist = RegimentManager.Instance.TryGetRegiment(Blackboard.EnemyTarget, out Regiment target);
            if (targetExist && IsTargetRegimentInRange(target))
            {
                return StateIdentity;
            }
            else if (CheckEnemiesAtRange(out int regimentTargeted))
            {
                Blackboard.EnemyTarget = regimentTargeted;
                return StateIdentity;
            }
            else
            {
                Blackboard.EnemyTarget = 0;
                return StateMachine.DefaultState;
            }
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ ConditionEnter ◈◈◈◈◈◈                                                                                   ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        /// <summary>
        /// return if current target is still in range
        /// </summary>
        public bool IsTargetRegimentInRange(Regiment regimentTargeted)
        {
            //FieldOfViewData fovData = GetFovData(LinkedRegiment, MaxRange, fovAngleInDegrees);
            return IsEnemyInRange(regimentTargeted);
        }
        
        // TODO : il faudra prendre en compte le nombre de cible a portée par rapport au nombre de tireur
        // TODO : Scenario to AVOID : prend pour cible le régiment d'une unités très en retard/isolée sur le groupe
        // TODO : Entrainera une tentative de tirer sur des troupes à l'autre bout du monde.
        
        /// <summary>
        /// Check for target in Range
        /// </summary>
        /// <returns>id of enemy target (0 = no target found)</returns>
        public bool CheckEnemiesAtRange(out int regimentTargeted)
        {
            NativeParallelMultiHashMap<int, float3> enemyRegimentToUnitsPositions = GetEnemiesPositions(LinkedRegiment);
            regimentTargeted = GetEnemiesDistancesSorted(enemyRegimentToUnitsPositions);
            return regimentTargeted != 0;
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Utility ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        private bool IsEnemyInRange(Regiment regimentTargeted)
        {
            FieldOfViewBounds fovBound = FieldOfView.Bounds;
            for (int i = 0; i < regimentTargeted.Count; i++)
            {
                float2 enemyUnitPosition2D = regimentTargeted[i].Position.xz;
                if (fovBound.Contains(enemyUnitPosition2D)) return true;
            }
            return false;
        }
        
        private int GetEnemiesDistancesSorted(NativeParallelMultiHashMap<int, float3> enemyRegimentToUnitsPositions)
        {
            int closestEnemyId = 0;
            float currentClosestEnemyDistance = float.MaxValue;
            
            float2 position2D = Position.xz;
            FieldOfViewBounds fovBound = FieldOfView.Bounds;
            foreach (KeyValue<int, float3> regimentToUnitPosition in enemyRegimentToUnitsPositions)
            {
                if (!regimentToUnitPosition.GetKeyValue(out int enemyRegimentId, out float3 enemyUnitPosition)) continue;
                
                // Is in field of view ?
                if (!fovBound.Contains(enemyUnitPosition.xz)) continue;
                
                // Check: Update Distance
                float distanceFromEnemy = distance(position2D, enemyUnitPosition.xz);
                if (distanceFromEnemy > currentClosestEnemyDistance) continue;
                
                closestEnemyId = enemyRegimentId;
                currentClosestEnemyDistance = distanceFromEnemy;
            }
            return closestEnemyId;
        }
        
        private static NativeParallelMultiHashMap<int, float3> GetEnemiesPositions(Regiment regimentAttach)
        {
            int enemyUnitsCount = RegimentManager.Instance.GetEnemiesTeamNumUnits(regimentAttach.TeamID);
            
            NativeParallelMultiHashMap<int, float3> temp = new (enemyUnitsCount, Temp);
            foreach ((int teamID, List<Regiment> regiments) in RegimentManager.Instance.RegimentsByTeamID)
            {
                if (teamID == regimentAttach.TeamID) continue;
                foreach (Regiment regiment in regiments)
                {
                    for (int i = 0; i < regiment.Count; i++)
                    {
                        temp.Add(regiment.RegimentID, regiment[i].Position);
                    }
                }
            }
            return temp;
        }
    }
}
