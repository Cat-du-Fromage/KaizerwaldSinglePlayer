using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald.StateMachine
{
    public abstract class StateSequencerBase<T> : StateBase<T>
    where T : StateMachineBase<T>
    {
        protected int SequenceIndex = 0;
        protected List<StateBase<T>> Sequence;
        
        protected StateSequencerBase(T stateMachine, EStates stateIdentity, List<StateBase<T>> sequence) : base(stateMachine, stateIdentity)
        {
            Sequence = sequence;
        }

        public override void OnSetup(Order order)
        {
            foreach (StateBase<T> state in Sequence)
            {
                state.OnSetup(order);
            }
        }

        public override void OnEnter()
        {
            SequenceIndex = 0;
            return;
        }

        public void OnNextSequence()
        {
            SequenceIndex++;
        }

        public override bool ShouldExit(out EStates state)
        {
            state = StateIdentity;
            return SequenceIndex == Sequence.Count - 1 && Sequence[^1].ShouldExit(out state);
        }
    }
}
