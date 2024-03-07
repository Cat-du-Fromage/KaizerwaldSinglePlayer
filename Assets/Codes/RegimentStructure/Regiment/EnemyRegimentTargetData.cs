using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Kaizerwald.FormationModule;

namespace Kaizerwald
{
    public class EnemyRegimentTargetData
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        private int enemyTargetID; // avoid Null Check by caching it
        private Regiment enemyTarget;
        private FormationData cacheEnemyFormation;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public bool IsTargetLocked { get; private set; }
        public Regiment EnemyTarget => enemyTarget;
        public int EnemyTargetID => enemyTargetID;
        public FormationData CacheEnemyFormation => cacheEnemyFormation;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public EnemyRegimentTargetData(Regiment enemyRegiment = null)
        {
            bool isTargetNull = enemyRegiment == null;
            enemyTarget = enemyRegiment;
            enemyTargetID = isTargetNull ? -1 : enemyRegiment.RegimentID;
            cacheEnemyFormation = isTargetNull ? default : enemyTarget.CurrentFormationData;
        }
        
        public EnemyRegimentTargetData(Regiment enemyRegiment, FormationData cacheEnemyFormation) : this(enemyRegiment)
        {
            this.cacheEnemyFormation = cacheEnemyFormation;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public bool IsTargetValid()
        {
            return enemyTargetID != -1 && RegimentManager.Instance.RegimentExist(enemyTargetID);
        }
        
        public void Clear()
        {
            enemyTarget = null;
            enemyTargetID = -1;
            IsTargetLocked = false;
        }

        public void SetEnemyTarget(int regimentID, bool lockTarget = false)
        {
            if (!RegimentManager.Instance.TryGetRegiment(regimentID, out enemyTarget)) return;
            IsTargetLocked = lockTarget;
            enemyTargetID = enemyTarget.RegimentID;
            cacheEnemyFormation = enemyTarget.CurrentFormationData;
        }
        
        public void SetEnemyTarget(Regiment enemyRegiment, bool lockTarget = false)
        {
            bool isTargetNull = enemyRegiment == null;
            if (isTargetNull) return;
            IsTargetLocked = lockTarget;
            enemyTarget = enemyRegiment;
            enemyTargetID = enemyRegiment.RegimentID;
            cacheEnemyFormation = enemyRegiment.CurrentFormationData;
        }

        public void LockTarget(bool enable)
        {
            IsTargetLocked = enable;
        }
        
        public void SetFormation(FormationData formation)
        {
            cacheEnemyFormation = formation;
        }

        public void UpdateCachedFormation()
        {
            if (cacheEnemyFormation.EqualComposition(enemyTarget.CurrentFormation)) return;
            cacheEnemyFormation = enemyTarget.CurrentFormation;
        }
    }
}
