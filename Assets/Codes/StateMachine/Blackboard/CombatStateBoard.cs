using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Kaizerwald.FormationModule;

namespace Kaizerwald.StateMachine
{
    
    public class CombatStateBoard
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        private FormationData cacheEnemyFormation;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public bool IsChasingTarget { get; private set; } = false;
        
        public int EnemyTargetID { get; private set; }
        public Regiment EnemyTarget { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void SetChasingTarget(bool enable)
        {
            IsChasingTarget = enable;
        }
            
        public void SetFormation(FormationData formation)
        {
            cacheEnemyFormation = formation;
        }
    
        public FormationData CacheEnemyFormation => cacheEnemyFormation;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public CombatStateBoard(Regiment enemyRegiment = null)
        {
            bool isTargetNull = enemyRegiment == null;
            EnemyTarget = enemyRegiment;
            EnemyTargetID = isTargetNull ? -1 : enemyRegiment.RegimentID;
            cacheEnemyFormation = isTargetNull ? default : EnemyTarget.CurrentFormationData;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public bool IsChasingValidTarget()
        {
            IsChasingTarget = IsChasingTarget && IsTargetValid();
            return IsChasingTarget;
        }
        
        public bool IsTargetValid()
        {
            return EnemyTargetID != -1 && RegimentManager.Instance.RegimentExist(EnemyTargetID);
        }
        
        public void Clear()
        {
            EnemyTarget = null;
            EnemyTargetID = -1;
            IsChasingTarget = false;
        }
        
        public void SetEnemyTarget(Regiment enemyTarget, bool chaseTarget = false)
        {
            EnemyTarget = enemyTarget;
            IsChasingTarget = chaseTarget;
            EnemyTargetID = EnemyTarget.RegimentID;
            cacheEnemyFormation = EnemyTarget.CurrentFormationData;
        }
        
        public bool TrySetEnemyTarget(int regimentID, bool chaseTarget = false)
        {
            if (!RegimentManager.Instance.TryGetRegiment(regimentID, out Regiment enemyTarget)) return false;
            SetEnemyTarget(enemyTarget, chaseTarget);
            return true;
        }
        
        public bool TrySetEnemyTarget(Regiment enemyTarget, bool chaseTarget = false)
        {
            if (enemyTarget == null) return false;
            SetEnemyTarget(enemyTarget, chaseTarget);
            return true;
        }
        
        public void UpdateCachedFormation()
        {
            if (cacheEnemyFormation.EqualComposition(EnemyTarget.CurrentFormation)) return;
            cacheEnemyFormation = EnemyTarget.CurrentFormation;
        }
    }
}
