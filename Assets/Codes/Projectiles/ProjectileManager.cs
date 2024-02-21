using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

using Kaizerwald.Utilities;

namespace Kaizerwald
{
    public class ProjectileManager : Singleton<ProjectileManager>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private bool initialized;
        
        //TODO MAKE POOL BY REGIMENT!!!! so we can destroy them
        private Dictionary<int, ObjectPool<ProjectileComponent>> RegimentBulletsPool = new();
        
        private List<ProjectileComponent> ActiveBullets = new(10);

        private HashSet<Unit> unitHits = new HashSet<Unit>(16);
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Awake | Start ◈◈◈◈◈◈                                                                                    ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        protected override void OnAwake()
        {
            base.OnAwake();
        }

        private void Start()
        {
            RegimentManager.Instance.OnNewRegiment += RegisterPool;
            initialized = true;
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Update | Late Update ◈◈◈◈◈◈                                                                             ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private void FixedUpdate()
        {
            if (unitHits.Count != 0)
            {
                HashSet<Unit> tmpUnitHits = new (unitHits);
                foreach (Unit unit in unitHits)
                {
                    unit.TriggerDeath();
                }
                unitHits.ExceptWith(tmpUnitHits);
            }
        }

        private void Update()
        {
            if (ActiveBullets.Count == 0) return;
            foreach (ProjectileComponent activeBullet in ActiveBullets)
            {
                activeBullet.OnUpdate();
            }
        }

        //A SURVEILLER DE PRES: POSSIBLE SOURCE DE BUG!
        private void LateUpdate()
        {
            if (ActiveBullets.Count == 0) return;
            CleanActiveBullets();
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Enable | Disable ◈◈◈◈◈◈                                                                                 ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        private void OnDisable()
        {
            if (RegimentManager.Instance == null) return;
            RegimentManager.Instance.OnNewRegiment -= RegisterPool;
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void RegisterUnitHits(Unit unitHit)
        {
            unitHits.Add(unitHit);
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Object Pooling Methods ◈◈◈◈◈◈                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public ProjectileComponent RequestBullet(Unit unit, Vector3 positionInRifle)
        {
            return RequestBullet(unit.LinkedRegiment.RegimentID, positionInRifle);
        }
        
        public ProjectileComponent RequestBullet(int regimentID, Vector3 positionInRifle)
        {
            return RegimentBulletsPool.TryGetValue(regimentID, out ObjectPool<ProjectileComponent> pool) ? pool.Pull(positionInRifle) : null;
        }
        
        public void RequestAndFireBullet(int regimentID, Vector3 positionInRifle, Vector3 direction)
        {
            if (!RegimentBulletsPool.TryGetValue(regimentID, out ObjectPool<ProjectileComponent> pool)) return;
            pool.Pull(positionInRifle).Fire(direction);
        }

        private void CleanActiveBullets()
        {
            for (int i = ActiveBullets.Count - 1; i > -1; i--)
            {
                if (ActiveBullets[i].isActiveAndEnabled) continue;
                ActiveBullets.RemoveAt(i);
            }
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Register / Unregister ◈◈◈◈◈◈                                                                            ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void RegisterPool(Regiment regiment)
        {
            GameObject prefab = regiment.RegimentType.BulletPrefab;
            //Remplacer par width??!! car seul la première ligne pouvant tirer?
            int unitCount = regiment.CurrentFormation.BaseNumUnits;
            RegimentBulletsPool.TryAdd(regiment.RegimentID, new ObjectPool<ProjectileComponent>(prefab, CallOnPull, unitCount));
        }
        
        private void CallOnPull(ProjectileComponent projectile)
        {
            ActiveBullets.Add(projectile);
        }
    }
}
