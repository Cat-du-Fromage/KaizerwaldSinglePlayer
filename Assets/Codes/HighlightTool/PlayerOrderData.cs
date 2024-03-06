using Kaizerwald.FormationModule;
using Kaizerwald.StateMachine;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace Kaizerwald
{
    public enum EOrderType : int
    {
        Move,
        Attack
    }
    
    public enum EMovePace : int
    {
        March,
        Run,
    }
    
    public struct PlayerOrderData
    {
        public int RegimentID;
        
        public EOrderType OrderType;
        public EMoveType MoveType;
        
        public float3 LeaderDestination;
        public FormationData TargetFormation;
        
        public int TargetEnemyID;
    }
}