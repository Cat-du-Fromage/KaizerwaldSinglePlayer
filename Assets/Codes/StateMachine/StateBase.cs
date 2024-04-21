using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace Kaizerwald.StateMachine
{
    public abstract class StateBase<T> where T : StateMachineBase<T>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public EStates StateIdentity { get; private set; }
        protected T StateMachine { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        protected float3 Position => StateMachine.Position;
        public Quaternion Rotation => StateMachine.Rotation;
        protected float3 Forward  => StateMachine.Forward;
        protected float3 Back     => StateMachine.Back;
        protected float3 Right    => StateMachine.Right;
        protected float3 Left     => StateMachine.Left;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected StateBase(T stateMachine, EStates stateIdentity)
        {
            StateMachine = stateMachine;
            StateIdentity = stateIdentity;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public virtual bool ConditionEnter() { return true; }

        //Specific to Player Order
        public abstract void OnSetup(Order order);
        
        public abstract void OnEnter();

        public virtual void OnFixedUpdate() { return;}
        
        public abstract void OnUpdate();
        
        public abstract void OnExit();
        
        public abstract bool ShouldExit(out EStates state);

        public virtual void OnDestroy() { return; }
    }
}
