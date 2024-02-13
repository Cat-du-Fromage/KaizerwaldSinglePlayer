using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace Kaizerwald.StateMachine
{
    public abstract class StateBase<T> where T : BehaviourTreeBase<T>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public EStates StateIdentity { get; private set; }
        protected T BehaviourTree { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        protected float3 Position => BehaviourTree.Position;
        protected float3 Forward  => BehaviourTree.Forward;
        protected float3 Back     => BehaviourTree.Back;
        protected float3 Right    => BehaviourTree.Right;
        protected float3 Left     => BehaviourTree.Left;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected StateBase(T behaviourTree, EStates stateIdentity)
        {
            BehaviourTree = behaviourTree;
            StateIdentity = stateIdentity;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public virtual bool ConditionEnter() { return true; }

        //Specific to Player Order
        public abstract void OnSetup(Order order);
        
        public abstract void OnEnter();
        
        public abstract void OnUpdate();
        
        public abstract void OnExit();
        
        public abstract EStates ShouldExit();

        public virtual void OnDestroy() { return; }
    }
}
