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

        private Regiment enemyTarget;
        private int enemyTargetID; // avoid Null Check by caching it
        private FormationData cacheEnemyFormation;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
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

        public void SetEnemyTarget(int regimentID)
        {
            enemyTargetID = -1;
            if (!RegimentManager.Instance.TryGetRegiment(regimentID, out enemyTarget)) return;
            enemyTargetID = enemyTarget.RegimentID;
            cacheEnemyFormation = enemyTarget.CurrentFormationData;
        }
        
        public void SetEnemyTarget(Regiment enemyRegiment)
        {
            bool isTargetNull = enemyRegiment == null;
            enemyTarget = enemyRegiment;
            enemyTargetID = isTargetNull ? -1 : enemyRegiment.RegimentID;
            cacheEnemyFormation = enemyRegiment.CurrentFormationData;
        }
        
        public void SetFormation(FormationData formation)
        {
            cacheEnemyFormation = formation;
        }
    }
}
