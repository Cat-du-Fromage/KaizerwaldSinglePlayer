using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

using Kaizerwald.Utilities;

namespace Kaizerwald
{
    [ExecuteBefore(typeof(RegimentManager), OrderDecrease = 1)]
    public class ProjectileManager : Singleton<ProjectileManager>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
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
    
        public void Start()
        {
            RegimentManager.Instance.OnNewRegiment += RegisterPool;
            RegimentManager.Instance.OnDeadRegiment += UnRegisterPool;
            //initialized = true;
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Update | Late Update ◈◈◈◈◈◈                                                                             ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public void FixedUpdate()
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

        public void Update()
        {
            if (ActiveBullets.Count == 0) return;
            foreach (ProjectileComponent activeBullet in ActiveBullets)
            {
                activeBullet.OnUpdate();
            }
        }

        public void LateUpdate()
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
            bool hasValue = RegimentBulletsPool.TryGetValue(regimentID, out ObjectPool<ProjectileComponent> pool);
            return hasValue ? pool.Pull(positionInRifle) : null;
        }

        private void CleanActiveBullets()
        {
            for (int i = ActiveBullets.Count - 1; i > -1; i--)
            {
                if (ActiveBullets[i].gameObject.activeSelf) continue;
                ActiveBullets.RemoveAt(i);
            }
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Register / Unregister ◈◈◈◈◈◈                                                                            ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void RegisterPool(Regiment regiment)
        {
            GameObject prefab = regiment.RegimentType.BulletPrefab;
            int unitCount = regiment.CurrentFormation.MaxRow;
            RegimentBulletsPool.TryAdd(regiment.RegimentID, new ObjectPool<ProjectileComponent>(prefab, CallOnPull, unitCount));
        }
        
        private void UnRegisterPool(GameObject regimentObject)
        {
            if (!regimentObject.TryGetComponent(out Regiment regiment)) return;
            //if (!RegimentBulletsPool.TryGetValue(regiment.RegimentID, out ObjectPool<ProjectileComponent> value)) return;
            RegimentBulletsPool.Remove(regiment.RegimentID);
        }
        
        private void CallOnPull(ProjectileComponent projectile)
        {
            ActiveBullets.Add(projectile);
        }
    }
}
