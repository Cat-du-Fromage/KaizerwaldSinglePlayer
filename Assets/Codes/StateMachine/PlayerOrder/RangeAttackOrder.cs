using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald.StateMachine
{
    public sealed class RangeAttackOrder : Order
    {
        public int TargetEnemyId { get; private set; }
        
        public RangeAttackOrder(PlayerOrderData playerOrder) : base(EStates.Fire)
        {
            TargetEnemyId = playerOrder.TargetEnemyID;
        }
    }
}
