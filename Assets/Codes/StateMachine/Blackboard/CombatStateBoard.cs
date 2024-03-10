using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Kaizerwald.FormationModule;

namespace Kaizerwald.StateMachine
{
    public enum ECombatMode
    {
        Range,
        Melee,
    }
    
    public class CombatStateBoard
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        private FormationData cacheEnemyFormation;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public ECombatMode CombatMode { get; private set; }
        public bool IsChasingTarget { get; private set; }
        
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
            if (IsChasingTarget && !IsTargetValid())
            {
                IsChasingTarget = false;
            }
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

        public void SetEnemyTarget(int regimentID, bool chaseTarget = false)
        {
            if (!RegimentManager.Instance.TryGetRegiment(regimentID, out Regiment enemyTarget)) return;
            EnemyTarget = enemyTarget;
            IsChasingTarget = chaseTarget;
            EnemyTargetID = EnemyTarget.RegimentID;
            cacheEnemyFormation = EnemyTarget.CurrentFormationData;
        }
        
        public void SetEnemyTarget(Regiment enemyTarget, bool chaseTarget = false)
        {
            //bool isTargetNull = enemyTarget == null;
            if (enemyTarget == null) return;
            IsChasingTarget = chaseTarget;
            EnemyTarget = enemyTarget;
            EnemyTargetID = enemyTarget.RegimentID;
            cacheEnemyFormation = enemyTarget.CurrentFormationData;
        }
        
        public void UpdateCachedFormation()
        {
            if (cacheEnemyFormation.EqualComposition(EnemyTarget.CurrentFormation)) return;
            cacheEnemyFormation = EnemyTarget.CurrentFormation;
        }
    }
}
