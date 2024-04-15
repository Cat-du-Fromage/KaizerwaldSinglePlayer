using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace Kaizerwald.StateMachine
{
    public abstract class StateMachineBase<T> : MonoBehaviour where T : StateMachineBase<T>
    {
        public readonly EStates DefaultState = EStates.Idle;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected bool IsInitialized;
        public Transform CachedTransform { get; protected set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [field:SerializeField] public EStates State { get; protected set; }
        public Dictionary<EStates, StateBase<T>> States { get; protected set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ State Access ◇◇◇◇◇◇                                                                                │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public StateBase<T> CurrentState => States[State];
        public bool IsIdle    => State == EStates.Idle;
        public bool IsMoving  => State == EStates.Move;
        public bool IsFiring  => State == EStates.Fire;
        public bool IsInMelee => State == EStates.Melee;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Transform Access ◇◇◇◇◇◇                                                                            │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public float3 Position => CachedTransform.position;
        public Quaternion Rotation => CachedTransform.rotation;
        public float3 Forward  => CachedTransform.forward;
        public float3 Back     => -CachedTransform.forward;
        public float3 Right    => CachedTransform.right;
        public float3 Left     => -CachedTransform.right;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝ 

        protected virtual void Awake()
        {
            CachedTransform = transform;
            State = EStates.Idle;
        }

        public virtual void OnUpdate()
        {
            if (TryChangeState()) return;
            CurrentState.OnUpdate();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public bool CanEnterState(EStates state)
        {
            return States[state].ConditionEnter();
        }

        protected bool TryChangeState()
        {
            if (!CurrentState.ShouldExit(out EStates nextState)) return false;
            CurrentState.OnExit();
            State = nextState;
            CurrentState.OnEnter();
            return true;
        }
        
        public virtual void RequestChangeState(Order order)
        {
            //Debug.Log($"RequestChangeState: was = {State}, then = {order.StateOrdered}");
            if (order.StateOrdered != State)
            {
                CurrentState.OnExit();
                State = order.StateOrdered;
            }
            CurrentState.OnSetup(order);
            CurrentState.OnEnter();
        }

        public virtual void RequestChangeState(Order order, bool overrideState)
        {
            if (order.StateOrdered == State && !overrideState) return;
            CurrentState.OnExit();
            State = order.StateOrdered;
            CurrentState.OnSetup(order);
            CurrentState.OnEnter();
        }

        protected abstract void InitializeStates();
    }
}