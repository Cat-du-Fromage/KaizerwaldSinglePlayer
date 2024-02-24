using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;

using static UnityEngine.LayerMask;
using static Unity.Mathematics.math;
using static Unity.Mathematics.int2;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using Random = Unity.Mathematics.Random;

using Kaizerwald.FormationModule;
using static Kaizerwald.Utilities.KzwMath;

namespace Kaizerwald.StateMachine
{
//------------------------------------------------------------------------------------------------------------------------------
    //TODO : REVOIR le système de visé (tir absurde trop bas(tir e contre bas à 1m) ou trop haut (vise les nuages)
    //TODO : (Optimisation) Potentiellement revoir la manière dont on tir, centralisé les évenement de tir dans une liste
//------------------------------------------------------------------------------------------------------------------------------
    
    public class UnitRangeAttackState : UnitStateBase<RegimentRangeAttackState>
    {
        private readonly LayerMask UnitLayerMask;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        //ATTENTION PAS { get; private set; } sinon NextFloat2Direction ne bougera pas
        private Random randomState;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public Unit UnitEnemyTarget { get; private set; }
        public float2 CurrentRandomAimDirection { get; private set; }
        public float3 AimDirection { get; private set; }
        //public FormationData CacheEnemyFormation { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public float3 UnitTargetPosition => UnitEnemyTarget.Position;
        
        // EnemyRegimentTargetData
        private EnemyRegimentTargetData EnemyRegimentTargetData => RegimentStateReference.EnemyRegimentTargetData;
        
        // RegimentType
        private int MaxRange => RegimentStateReference.MaxRange;
        private int Accuracy => RegimentStateReference.Accuracy;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public UnitRangeAttackState(UnitBehaviourTree behaviourTree) : base(behaviourTree, EStates.Fire)
        {
            UnitLayerMask = RegimentManager.Instance.UnitLayerMask;

            uint seed = (uint)(abs(LinkedUnit.GetInstanceID()) + IndexInFormation);
            randomState = Random.CreateFromIndex(seed);
            CurrentRandomAimDirection = randomState.NextFloat2Direction();
            
            UnitAnimation.OnShootEvent += OnFireEvent;
        }

        public override bool ConditionEnter()
        {
            bool regimentIsFiring = LinkedRegimentBehaviourTree.IsFiring;
            bool isFirstLine = IndexInFormation < RegimentStateReference.CurrentFormation.Width;
            return regimentIsFiring && isFirstLine;
        }

        public override void OnSetup(Order order)
        {
            TryGetEnemyTarget(out Unit unit);
            UnitEnemyTarget = unit;
        }

        public override void OnEnter()
        {
            if (TryGetEnemyTarget(out Unit unit))
            {
                UnitEnemyTarget = unit;
                UnitAnimation.SetFullFireSequenceOn();
            }
        }

        public override void OnUpdate()
        {
            Retarget();
            UnitTakeAim();
        }

        public override void OnExit()
        {
            UnitAnimation.SetFullFireSequenceOff();
        }

        public override EStates ShouldExit()
        {
            // IL FAUT FORCER LE REARRANGEMENT
            return IsRegimentStateIdentical ? StateIdentity : RegimentState;
        }
        
        protected override EStates TryReturnToRegimentState()
        {
            return StateIdentity;
        }
        
        public override void OnDestroy()
        {
            UnitAnimation.OnShootEvent -= OnFireEvent;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void Retarget()
        {
            if (IsTargetValid()) return;
            if (TryGetEnemyTarget(out Unit unit))
            {
                UnitEnemyTarget = unit;
            }
        }
        
        private bool IsTargetValid()
        {
            return UnitEnemyTarget != null && !UnitEnemyTarget.IsInactive;
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Animation Event ◈◈◈◈◈◈                                                                                  ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        // Call : objectAttach.Animation.OnShootEvent
        private void OnFireEvent(AnimationEvent animationEvent)
        {
            Vector3 bulletStartPosition = Position + up() + Forward;
            //if (Physics.Raycast(bulletStartPosition, AimDirection, MaxRange, UnitLayerMask.value))
            //{
                int regimentID = LinkedParentRegiment.RegimentID;
                ProjectileManager.Instance.RequestBullet(regimentID, bulletStartPosition).Fire(AimDirection);
                //ProjectileManager.Instance.RequestAndFireBullet(regimentID, bulletStartPosition, AimDirection);
            //}
            CurrentRandomAimDirection = randomState.NextFloat2Direction(); // Renew Random Direction
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Shoot Sequence ◈◈◈◈◈◈                                                                                   ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private void UnitTakeAim()
        {
            if (UnitAnimation.IsPlayingReload) return;
            float3 directionUnitToTarget = normalizesafe(UnitTargetPosition - Position);
            //Only on x and y axis (forward(z) axis dont have any value)
            float3 randomDirection = new float3(CurrentRandomAimDirection * (Accuracy / 10f), 0);
            float3 maxRangePosition = Position + MaxRange * directionUnitToTarget;
            float3 spreadEndPoint = maxRangePosition + randomDirection;
            AimDirection = normalizesafe(spreadEndPoint - Position);
            CheckUnitIsFiring();
        }
        
        private void CheckUnitIsFiring()
        {
            if (UnitAnimation.IsInFiringMode) return;
            UnitAnimation.SetFullFireSequenceOn();
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Get Target Methods ◇◇◇◇◇◇                                                                          │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private bool TryGetEnemyTarget(out Unit target)
        {
            //EnemyRegimentTargetData regimentTargetData = LinkedParentRegiment.EnemyRegimentTargetData;
            using NativeHashSet<int> hullIndices = GetUnitsHullIndices(EnemyRegimentTargetData.CacheEnemyFormation);
            
            (int index, float minDistance) = (-1, INFINITY);
            foreach (int unitIndex in hullIndices)
            {
                //if (regimentTargetData.EnemyTarget[unitIndex] == null) continue;
                float2 enemyPosition = EnemyRegimentTargetData.EnemyTarget[unitIndex].Position.xz;
                float distanceToTarget = distancesq(Position.xz, enemyPosition);
                if (distanceToTarget > minDistance) continue;
                (index, minDistance) = (unitIndex, distanceToTarget);
            }
            
            bool hasTarget = index > -1;
            target = !hasTarget ? null : EnemyRegimentTargetData.EnemyTarget[index];
            return hasTarget;
        }
        
        private NativeHashSet<int> GetUnitsHullIndices(in FormationData enemyFormation)
        {
            (int numUnit, int width) = (enemyFormation.NumUnitsAlive, enemyFormation.Width);
            int2 maxWidthDepth = enemyFormation.WidthDepth - 1;
            
            //TODO Vérifier si Correct!
            int numIndices = max(enemyFormation.NumUnitsLastLine, enemyFormation.NumCompleteLine * width);
            
            NativeHashSet<int> indices = new (numIndices, Temp);
            for (int i = 0; i < numUnit; i++)
            {
                int2 coords = GetXY2(i, width);
                if (!any(coords == zero) && !any(coords == maxWidthDepth)) continue; 
                indices.Add(i);
            }
            return indices;
        }
    }
}
