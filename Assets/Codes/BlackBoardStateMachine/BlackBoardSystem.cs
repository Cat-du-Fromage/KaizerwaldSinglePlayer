using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    public class BlackBoardSystem : MonoBehaviour
    {
        private Dictionary<int, RegimentBlackboard> RegimentBlackboards = new (8);
        
        public void Register(GameObject stateMachine, RegimentBlackboard blackboard)
        {
            RegimentBlackboards.TryAdd(stateMachine.GetInstanceID(), blackboard);
        }
        
        public void UnRegister(GameObject stateMachine)
        {
            RegimentBlackboards.Remove(stateMachine.GetInstanceID());
        }

        private void Test()
        {
            foreach ((int stateMachineId, RegimentBlackboard blackboard) in RegimentBlackboards)
            {
                DynamicStatusBoard statusBoard = blackboard.DynamicStatusBoard;
                if (statusBoard.HasTarget && !RegimentManager.Instance.RegimentExist(statusBoard.TargetEnemyId))
                {
                    statusBoard.HasTarget = false;
                    statusBoard.TargetEnemyId = -1;
                    if (statusBoard is { IsChasing: true, IsMoving: true })
                    {
                        RegimentManager.Instance.RegimentsByID[stateMachineId].ResetDestination();
                        statusBoard.IsChasing = false;
                        statusBoard.IsMoving = false;
                    }
                }
            }
        }
    }
}
