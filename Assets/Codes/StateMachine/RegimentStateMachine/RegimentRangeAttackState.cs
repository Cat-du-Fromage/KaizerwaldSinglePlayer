using System;
using System.Collections;
using System.Collections.Generic;
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
using static Kaizerwald.Utilities.KzwMath;

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
                return CheckEnemiesAtRange(RegimentManager.RegimentFieldOfView, out Blackboard.EnemyTarget);
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
            if (targetExist && IsTargetRegimentInRange(target, RegimentManager.RegimentFieldOfView))
            {
                return StateIdentity;
            }
            else if (CheckEnemiesAtRange(RegimentManager.RegimentFieldOfView, out int regimentTargeted))
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
        /// Check if current target is still in range
        /// </summary>
        /// <param name="regimentTargeted">Current Target</param>
        /// <param name="fovAngleInDegrees">Angle of the Cone of the Field of view</param>
        /// <returns></returns>
        public bool IsTargetRegimentInRange(Regiment regimentTargeted, float fovAngleInDegrees)
        {
            FieldOfViewData fovData = GetFovData(LinkedRegiment, MaxRange, fovAngleInDegrees);
            return IsEnemyInRange(LinkedRegiment, regimentTargeted, fovData);
        }
        
        // TODO : il faudra prendre en compte le nombre de cible a portée par rapport au nombre de tireur
        // TODO : Scenario to AVOID : prend pour cible le régiment d'une unités très en retard/isolée sur le groupe
        // TODO : Entrainera une tentative de tirer sur des troupes à l'autre bout du monde.
        
        /// <summary>
        /// Check for target in Range
        /// </summary>
        /// <param name="fovAngleInDegrees">Angle of the Cone of the Field of view</param>
        /// <param name="regimentTargeted">gameobject instance Id of the target found</param>
        /// <returns>id of enemy target (0 = no target found)</returns>
        public bool CheckEnemiesAtRange(float fovAngleInDegrees, out int regimentTargeted)
        {
            FieldOfViewData fovData = GetFovData(LinkedRegiment, MaxRange, fovAngleInDegrees);
            NativeParallelMultiHashMap<int, float3> enemyRegimentToUnitsPositions = GetEnemiesPositions(LinkedRegiment);
            regimentTargeted = GetEnemiesDistancesSorted(LinkedRegiment, fovData, enemyRegimentToUnitsPositions);
            return regimentTargeted != 0;
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ SHARED ◇◇◇◇◇◇                                                                                      │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private FieldOfViewData GetFovData(Regiment regimentAttach, int attackRange, float fovAngleInDegrees)
        {
            //from center of the first Row : direction * midWidth length(Left and Right)
            FormationData formationData = regimentAttach.CurrentFormationData;
            float formationWidthLength = formationData.DistanceUnitToUnitX * (formationData.Width - 1) + formationData.UnitSize.x; //if width == 1 => UnitSize is used
            
            float2 midWidthDistance = regimentAttach.Right.xz * formationWidthLength / 2f;
            
            float2 unitLeft = regimentAttach.Position.xz - midWidthDistance; //unit most left
            float2 unitRight = regimentAttach.Position.xz + midWidthDistance; //unit most Right
                
            //Rotation of the direction the regiment is facing (around Vector3.up) to get both direction of the vision cone
            float2 directionLeft = mul(AngleAxis(-fovAngleInDegrees, up()), regimentAttach.Forward).xz;
            float2 directionRight = mul(AngleAxis(fovAngleInDegrees, up()), regimentAttach.Forward).xz;
            
            //Get tip of the cone formed by the intersection made by the 2 previous directions calculated
            float2 intersection = GetIntersection(unitLeft, unitRight, directionLeft, directionRight);
            float radius = attackRange + distance(intersection, unitLeft);
            
            return new FieldOfViewData(unitLeft, unitRight, directionLeft, directionRight, intersection, radius);
        }

        private bool IsInsideFieldOfView(float2 position2D, float2 forward2D, float2 enemyUnitPosition2D, in FieldOfViewData fovData)
        {
            // 1) Is Inside The Circle (Range)
            float distanceFromEnemy = distance(fovData.TriangleTip, enemyUnitPosition2D);
            if (distanceFromEnemy > fovData.Radius) return false; // isEnemyOutOfRange
                
            // 2) Behind Regiment Check
            //Regiment.forward: (regPos -> directionForward) , regiment -> enemy: (enemyPos - regPos) 
            float2 regimentToEnemyUnitDirection = normalizesafe(enemyUnitPosition2D - position2D);
            if (dot(regimentToEnemyUnitDirection, forward2D) < 0) return false; // isEnemyBehind
            
            // 3) Is Inside the Triangle of vision (by checking inside both circle and triangle we get the Cone)
            float2 topForwardDirection = normalizesafe(position2D - fovData.TriangleTip);
            float2 topForwardFov = fovData.TriangleTip + topForwardDirection * fovData.Radius;
            
            return enemyUnitPosition2D.IsPointInTriangle
            (
                fovData.TriangleTip, 
                GetIntersection(topForwardFov, fovData.LeftStartPosition, topForwardDirection.CrossLeft(), fovData.LeftDirection), 
                GetIntersection(topForwardFov, fovData.RightStartPosition, topForwardDirection.CrossRight(), fovData.RightDirection)
            );
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Utility ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        private bool IsEnemyInRange(Regiment regimentAttach, Regiment regimentTargeted, in FieldOfViewData fovData)
        {
            float2 position2D = regimentAttach.Position.xz;
            float2 forward2D = regimentAttach.Forward.xz;
            for (int i = 0; i < regimentTargeted.Count; i++)
            {
                float2 enemyUnitPosition2D = regimentTargeted[i].Position.xz;
                // is in field of view ?
                if (!IsInsideFieldOfView(position2D, forward2D, enemyUnitPosition2D, fovData)) continue;
                return true;
            }
            return false;
        }
        
        private int CountUnitsInRange(Regiment regimentAttach, Regiment regimentTargeted, in FieldOfViewData fovData)
        {
            int numUnitsInRange = 0;
            float2 position2D = regimentAttach.Position.xz;
            float2 forward2D = regimentAttach.Forward.xz;
            for (int i = 0; i < regimentTargeted.Count; i++)
            {
                float2 enemyUnitPosition2D = regimentTargeted[i].Position.xz;
                numUnitsInRange += IsInsideFieldOfView(position2D, forward2D, enemyUnitPosition2D, fovData) ? 1 : 0;
            }
            return numUnitsInRange;
        }
        
        private int GetEnemiesDistancesSorted(Regiment regimentAttach, in FieldOfViewData fovData, NativeParallelMultiHashMap<int, float3> enemyRegimentToUnitsPositions)
        {
            int closestEnemyId = 0;
            float currentClosestEnemyDistance = float.MaxValue;
            
            float2 position2D = regimentAttach.Position.xz;
            float2 forward2D = regimentAttach.Forward.xz;
            
            //NativeParallelMultiHashMap<int, float3> enemyRegimentToUnitsPositions = GetEnemiesPositions(regimentAttach);
            foreach (KeyValue<int, float3> regimentToUnitPosition in enemyRegimentToUnitsPositions)
            {
                if (!regimentToUnitPosition.GetKeyValue(out int enemyRegimentId, out float3 enemyUnitPosition)) continue;
                
                // Is in field of view ?
                if (!IsInsideFieldOfView(position2D, forward2D, enemyUnitPosition.xz, fovData)) continue;
                
                // Check: Update Distance
                float distanceFromEnemy = distance(fovData.TriangleTip, enemyUnitPosition.xz);
                if (distanceFromEnemy > currentClosestEnemyDistance) continue;
                
                closestEnemyId = enemyRegimentId;
                currentClosestEnemyDistance = distanceFromEnemy;
            }
            return closestEnemyId;
        }
        
        private static NativeParallelMultiHashMap<int, float3> GetEnemiesPositions(Regiment regimentAttach)
        {
            int numEnemyUnits = RegimentManager.Instance.GetEnemiesTeamNumUnits(regimentAttach.TeamID);
            
            NativeParallelMultiHashMap<int, float3> temp = new (numEnemyUnits, Temp);
            foreach ((int teamID, List<Regiment> regiments) in RegimentManager.Instance.RegimentsByTeamID)
            {
                if (teamID == regimentAttach.TeamID) continue;
                foreach (Regiment regiment in regiments)
                {
                    //regiment.Transforms.ForEach(unitTransform => temp.Add(regiment.RegimentID, unitTransform.position));
                    for (int i = 0; i < regiment.Count; i++)
                    {
                        temp.Add(regiment.RegimentID, regiment[i].Position);
                    }
                }
            }
            return temp;
        }
    }
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Jobify Utility ◇◇◇◇◇◇                                                                              │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        /*
        private bool CheckEnemiesAtRangeByJob(Regiment regimentAttach, in FieldOfViewData fovData, out int regimentTargeted)
        {
            int numEnemyRegiments = RegimentManager.Instance.CountEnemyTeamRegiments(regimentAttach.TeamID);
            int numEnemyUnits = RegimentManager.Instance.GetEnemiesTeamNumUnits(regimentAttach.TeamID);
            
            NativeParallelMultiHashMap<int, float3> enemyUnitsPositions = new(numEnemyUnits, TempJob);
            JobHandle dependency = GetEnemiesPositionsByJob(regimentAttach, numEnemyRegiments, enemyUnitsPositions);
            
            NativeHashMap<int, float> enemyRegimentDistances = new (numEnemyRegiments, TempJob);
            JCheckInsideFOV jobCheckInsideFOV = new JCheckInsideFOV
            {
                RegimentPosition2D = regimentAttach.Position.xz,
                RegimentForward2D = regimentAttach.Forward.xz,
                FovData = fovData,
                RegimentToUnitsPosition = enemyUnitsPositions.GetKeyValueArrays(TempJob),
                EnemyRegimentDistances = enemyRegimentDistances
            };
            JobHandle d2 = jobCheckInsideFOV.Schedule(numEnemyRegiments, dependency);
            d2.Complete();
            
            bool targetFound = !enemyRegimentDistances.IsEmpty;
            regimentTargeted = targetFound ? enemyRegimentDistances.GetKeyMinValue() : -1;
            enemyRegimentDistances.Dispose();
            return targetFound;
        }

        private struct JCheckInsideFOV : IJobFor
        {
            [ReadOnly] public float2 RegimentPosition2D;
            [ReadOnly] public float2 RegimentForward2D;
            [ReadOnly] public FieldOfViewData FovData;
            
            [ReadOnly, DeallocateOnJobCompletion] public NativeKeyValueArrays<int, float3> RegimentToUnitsPosition;
            [WriteOnly] public NativeHashMap<int, float> EnemyRegimentDistances;
            
            public void Execute(int index)
            {
                int enemyRegimentId = RegimentToUnitsPosition.Keys[index];
                float3 enemyUnitPosition = RegimentToUnitsPosition.Values[index];
                // Is in field of view ?
                if (!IsInsideFieldOfViewJob(enemyUnitPosition.xz)) return;
            
                // Check: Update Distance
                float distanceFromEnemy = distance(FovData.TriangleTip, enemyUnitPosition.xz);
                bool keyExist = EnemyRegimentDistances.TryGetValue(enemyRegimentId, out float currentMinDistance);
                if (!keyExist || distanceFromEnemy < currentMinDistance) return;
                EnemyRegimentDistances[enemyRegimentId] = distanceFromEnemy;
            }
            
            private bool IsInsideFieldOfViewJob(float2 enemyUnitPosition2D)
            {
                // 1) Is Inside The Circle (Range)
                float distanceFromEnemy = distance(FovData.TriangleTip, enemyUnitPosition2D);
                if (distanceFromEnemy > FovData.Radius) return false; // isEnemyOutOfRange
                
                // 2) Behind Regiment Check
                //Regiment.forward: (regPos -> directionForward) , regiment -> enemy: (enemyPos - regPos) 
                float2 regimentToEnemyUnitDirection = normalizesafe(enemyUnitPosition2D - RegimentPosition2D);
                if (dot(regimentToEnemyUnitDirection, RegimentForward2D) < 0) return false; // isEnemyBehind
            
                // 3) Is Inside the Triangle of vision (by checking inside both circle and triangle we get the Cone)
                float2 topForwardDirection = normalizesafe(RegimentPosition2D - FovData.TriangleTip);
                float2 topForwardFov = FovData.TriangleTip + topForwardDirection * FovData.Radius;
                return enemyUnitPosition2D.IsPointInTriangle
                (
                    FovData.TriangleTip, 
                    GetIntersection(topForwardFov, FovData.LeftStartPosition, topForwardDirection.CrossLeft(), FovData.LeftDirection), 
                    GetIntersection(topForwardFov, FovData.RightStartPosition, topForwardDirection.CrossRight(), FovData.RightDirection)
                );
            }
        }
        
        private static JobHandle GetEnemiesPositionsByJob(Regiment regimentAttach, int numEnemyRegiments, NativeParallelMultiHashMap<int, float3> container)
        {
            NativeArray<JobHandle> jobHandles = new(numEnemyRegiments, Temp, UninitializedMemory);
            foreach ((int teamID, List<Regiment> regiments) in RegimentManager.Instance.RegimentsByTeamID)
            {
                if (teamID == regimentAttach.TeamID) continue;
                for (int i = 0; i < regiments.Count; i++)
                {
                    Regiment regiment = regiments[i];
                    JobHandle dependency = i == 0 ? default : jobHandles[i - 1];
                    JGetEnemyUnits job = new JGetEnemyUnits
                    {
                        RegimentID = regiment.RegimentID,
                        RegimentToUnitsPosition = container.AsParallelWriter()
                    };
                    jobHandles[i] = job.ScheduleReadOnly(regiment.FormationTransformAccessArray, regiment.Count, dependency);
                }
            }
            return JobHandle.CombineDependencies(jobHandles);
        }
        
        [BurstCompile]
        private struct JGetEnemyUnits : IJobParallelForTransform
        {
            [ReadOnly] public int RegimentID;
            [WriteOnly] public NativeParallelMultiHashMap<int, float3>.ParallelWriter RegimentToUnitsPosition;
            
            public void Execute(int index, TransformAccess transform)
            {
                if (!transform.isValid) return;
                RegimentToUnitsPosition.Add(RegimentID, transform.position);
            }
        }
    
        */
        /*
        private float2x3 GetTrianglePoints(float2 position2D, in FieldOfViewData fovData)
        {
            float2 topForwardDirection = normalizesafe(position2D - fovData.TriangleTip);
            float2 topForwardFov = fovData.TriangleTip + topForwardDirection * fovData.Radius;
            float2x3 points = new()
            {
                c0 = fovData.TriangleTip,
                c1 = GetIntersection(topForwardFov, fovData.LeftStartPosition, topForwardDirection.CrossLeft(), fovData.LeftDirection),
                c2 = GetIntersection(topForwardFov, fovData.RightStartPosition, topForwardDirection.CrossRight(), fovData.RightDirection)
            };
            return points;
        }
        */
}
