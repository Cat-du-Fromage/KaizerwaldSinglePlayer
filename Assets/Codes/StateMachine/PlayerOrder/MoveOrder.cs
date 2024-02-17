using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Kaizerwald.FormationModule;

namespace Kaizerwald.StateMachine
{
    public enum EMoveType : int
    {
        None = 0,
        March = 1,
        Run = 2,
    }
    
    public sealed class MoveOrder : Order
    {
        public EMoveType MoveType { get; private set; }
        public FormationData TargetFormation { get; private set; }
        public float3 LeaderTargetPosition { get; private set; }
        
        public MoveOrder(FormationData targetFormation, float3 leaderTargetPosition, EMoveType moveType = EMoveType.Run) : base(EStates.Move)
        {
            TargetFormation = targetFormation;
            LeaderTargetPosition = leaderTargetPosition;
            MoveType = moveType;
        }

        public override string ToString()
        {
            return $"LeaderDestination: {LeaderTargetPosition}\n {TargetFormation.ToString()}";
        }
    }
}
