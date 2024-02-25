using System;

namespace Kaizerwald
{
    public interface IGameSystem : IComparable<IGameSystem>
    {
        public int ExecutionOrderWeight { get; }
        
        public void OnStart();
        
        public void OnFixedUpdate();
        public void OnUpdate();
        public void OnLateUpdate();

        int IComparable<IGameSystem>.CompareTo(IGameSystem other)
        {
            return ExecutionOrderWeight.CompareTo(other.ExecutionOrderWeight);
        }
    }
}