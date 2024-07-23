using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kaizerwald.FormationModule;

namespace Kaizerwald.StateMachine
{
    public abstract class RegimentSequencerStateBase : StateSequencerBase<RegimentStateMachine>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public Regiment LinkedRegiment   => StateMachine.LinkedRegiment;
        public RegimentType RegimentType => LinkedRegiment.RegimentType;
        
        public Formation CurrentFormation => LinkedRegiment.CurrentFormation;
        public Formation TargetFormation  => LinkedRegiment.TargetFormation;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected RegimentSequencerStateBase(RegimentStateMachine stateMachine, EStates stateIdentity, List<StateBase<RegimentStateMachine>> sequence) 
            : base(stateMachine, stateIdentity, sequence)
        {
        }
    }
}
