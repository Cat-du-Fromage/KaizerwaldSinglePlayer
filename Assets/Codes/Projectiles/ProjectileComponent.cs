using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using Random = Unity.Mathematics.Random;

using Kaizerwald.Utilities;

namespace Kaizerwald
{
    public class ProjectileComponent : MonoBehaviour, IPoolable<ProjectileComponent>
    {
        private const float MaxDistance = 1024f;
        private const float Velocity = 5f;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [field:SerializeField] public EBulletType BulletType { get; private set; }
        
        [SerializeField] private float MuzzleVelocity = 10f;
        //[SerializeField] private LayerMask UnitLayerMask;
        private int unitLayerIndex;
        
        [SerializeField] private Rigidbody BulletRigidbody;
        [SerializeField] private TrailRenderer Trail;
        
        private float3 startPosition;
        
        private Unit enemyHit;
        private Transform bulletTransform;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Events ◈◈◈◈◈◈                                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private Action<ProjectileComponent> returnToPool;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void Awake()
        {
            bulletTransform = transform;
            BulletRigidbody = GetComponent<Rigidbody>();
            Trail = GetComponent<TrailRenderer>();
            unitLayerIndex = RegimentManager.Instance.UnitLayerMask.GetLayerIndex();
        }
        
        public void OnUpdate()
        {
            if (BulletRigidbody.velocity != Vector3.zero && !CheckFadeDistance()) return;
            ReturnToPool();
        }

        private void OnCollisionEnter(Collision other)
        {
            bool hit = other.gameObject.layer == unitLayerIndex;
            if (!hit || !CheckHasUnitComponent(other.gameObject, out Unit unit)) return;
            if (!unit.IsInactive)
            {
                //unit.TriggerDeath();
                ProjectileManager.Instance.RegisterUnitHits(unit);
            }
            ReturnToPool();
        }

        private void RegisterHitUnitToRegiment(Unit unit) => unit.LinkedRegiment.OnDeadUnit(unit);
        
        private bool CheckHasUnitComponent(GameObject hitGameObject, out Unit unit)
        {
            if (hitGameObject.TryGetComponent(out unit)) return true;
#if UNITY_EDITOR
            Debug.LogError($"Hit GameObject: {hitGameObject.name} with unitLayer: {unitLayerIndex} but dont have Unit Component");
#endif
            return false;
        }
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ IPoolable Interface ◈◈◈◈◈◈                                                                              ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void Initialize(Action<ProjectileComponent> returnAction)
        {
            returnToPool = returnAction;
        }

        public void ReturnToPool()
        {
            returnToPool.Invoke(this);
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Firing Behaviour ◈◈◈◈◈◈                                                                                 ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private bool CheckFadeDistance()
        {
            return distancesq(bulletTransform.position, startPosition) > MaxDistance;
        }

        public void MakeReady(Vector3 positionInRifle)
        {
            bulletTransform.position = positionInRifle;
        }
        
        public void Fire(Vector3 direction)
        {
            BulletRigidbody.velocity = direction * Velocity;
            BulletRigidbody.useGravity = true;
            Trail.emitting = true;
            BulletRigidbody.AddForce(BulletRigidbody.velocity * MuzzleVelocity, ForceMode.Impulse);
        }

        public void Fire(Vector3 positionInRifle, Vector3 direction)
        {
            startPosition = positionInRifle;
            Fire(direction);
        }
    }
}
