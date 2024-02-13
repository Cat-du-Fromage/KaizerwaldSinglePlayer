using UnityEngine;

namespace Kaizerwald.StateMachine
{
    public class Order
    {
        public static Order Default { get; private set; } = new Order(EStates.Idle);
        
        public EStates StateOrdered { get; protected set; }
        
        protected Order(EStates state)
        {
            StateOrdered = state;
        }
    }
}