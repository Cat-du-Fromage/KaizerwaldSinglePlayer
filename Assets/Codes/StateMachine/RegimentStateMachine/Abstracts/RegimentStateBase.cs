using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using Kaizerwald.FormationModule;

namespace Kaizerwald.StateMachine
{
    public abstract class RegimentStateBase : StateBase<RegimentStateMachine>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public Regiment LinkedRegiment => StateMachine.LinkedRegiment;
        public RegimentType RegimentType => LinkedRegiment.RegimentType;
        
        public Formation CurrentFormation => LinkedRegiment.CurrentFormation;
        public Formation TargetFormation => LinkedRegiment.TargetFormation;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected RegimentStateBase(RegimentStateMachine stateMachine, EStates stateIdentity) : base(stateMachine, stateIdentity)
        {
            
        }
    }
}