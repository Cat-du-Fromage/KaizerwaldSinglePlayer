#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Kaizerwald.FormationModule;
using Kaizerwald.StateMachine;
using Kaizerwald.Utilities;
using Unity.Mathematics;

namespace Kaizerwald
{
    
    public sealed partial class Unit : FormationElementBehaviour
    {
        //private bool DebugAvailable = false;
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            Debug_FireState();
        }

        private void Debug_FireState()
        {
            if (!StateMachine.IsFiring || IsInactive) return;
            UnitRangeAttackState fireState = (UnitRangeAttackState)StateMachine.CurrentState;
            ShowGizmosTarget(fireState);
            ShowAimTarget(fireState);
        }
        
        private void ShowGizmosTarget(UnitRangeAttackState fireState)
        {
            if (!Regiment.ShowTargetsFiringStateTest || fireState.UnitEnemyTarget == null) return;
            DrawArrow.HandleLine(Position, fireState.UnitEnemyTarget.Position, Color.red,1f, 0.5f);
        }
        
        private void ShowAimTarget(UnitRangeAttackState fireState)
        {
            if (!Animation.IsInAimingMode || fireState.UnitEnemyTarget == null) return;
            float distanceUnitToTarget = math.distance(Position, fireState.UnitEnemyTarget.Position);
            float3 endPosition = Position + fireState.AimDirection * distanceUnitToTarget;
            DrawArrow.HandleLine(Position, endPosition, Color.magenta,1f, 0.5f);
        }
    }
}
#endif