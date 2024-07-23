using Kaizerwald.FormationModule;
using Unity.Mathematics;

namespace Kaizerwald.StateMachine
{
    public struct Order
    {
        public EStates StateOrdered;
        public int EnemyTargetId;
        public float3 TargetPosition;
        public FormationData TargetFormation;
    }
}