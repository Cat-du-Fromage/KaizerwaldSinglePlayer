using UnityEngine;

namespace Kaizerwald.StateMachine
{
    public class MoveUnitState : MoveState
    {
        private MoveRegimentState RegimentState;
        
        public MoveUnitState(Transform transform, MoveRegimentState regimentState) : base(transform)
        {
            RegimentState = regimentState;
        }
    }
}