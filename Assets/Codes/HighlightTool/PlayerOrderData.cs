using Kaizerwald.FormationModule;
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
        public EMovePace MovePace;
        
        public float3 LeaderDestination;
        public FormationData TargetFormation;
        
        public int TargetEnemyID;
    }
}