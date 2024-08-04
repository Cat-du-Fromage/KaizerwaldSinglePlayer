using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald.StateMachine
{
    public abstract class MoveState : State<EStates> 
    {
        public enum MoveSteps
        {
            Redirection,
            Move
        }

        protected Transform Transform { get; private set; }
        
        protected MoveState(Transform transform) : base(EStates.Move)
        {
            Transform = transform;
        }
    }
}
