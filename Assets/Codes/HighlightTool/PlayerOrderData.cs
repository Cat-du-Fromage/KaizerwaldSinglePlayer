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
    
    public struct PlayerOrderData
    {
        public int RegimentID;
        
        public EOrderType OrderType;
        public bool IsRunning;
        
        public float3 LeaderDestination;
        public FormationData TargetFormation;
        
        public int TargetEnemyID;
    }
    
}