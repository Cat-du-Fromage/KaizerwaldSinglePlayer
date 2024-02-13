using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using Kaizerwald.FormationModule;

namespace Kaizerwald.StateMachine
{
    public abstract class RegimentStateBase : StateBase<RegimentBehaviourTree>
    {
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public Regiment LinkedRegiment => BehaviourTree.LinkedRegiment;
        public RegimentType RegimentType => LinkedRegiment.RegimentType;
        //public OrderedFormationBehaviour<Unit> FormationMatrix => BehaviourTree.RegimentAttach;
        public Formation CurrentFormation => LinkedRegiment.CurrentFormation;
        public Formation TargetFormation => LinkedRegiment.TargetFormation;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Setters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected RegimentStateBase(RegimentBehaviourTree behaviourTree, EStates stateIdentity) : base(behaviourTree, stateIdentity)
        {
            
        }
    }
}
