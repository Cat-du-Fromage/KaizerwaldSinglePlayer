using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald.StateMachine
{
    public sealed class RangeAttackOrder : Order
    {
        public Regiment TargetEnemyRegiment { get; private set; }
        public RangeAttackOrder(Regiment enemyRegiment) : base(EStates.Fire)
        {
            TargetEnemyRegiment = enemyRegiment;
        }
    }
}
