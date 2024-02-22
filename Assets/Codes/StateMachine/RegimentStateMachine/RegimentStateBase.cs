using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using Kaizerwald.FormationModule;

namespace Kaizerwald.StateMachine
{
    public abstract class RegimentStateBase : StateBase<RegimentBehaviourTree>
    {
        protected const float FOV_ANGLE = RegimentManager.RegimentFieldOfView;
        protected const float REACH_DISTANCE_THRESHOLD = 0.0125f;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public Regiment LinkedRegiment => BehaviourTree.LinkedRegiment;
        public RegimentType RegimentType => LinkedRegiment.RegimentType;
        public EnemyRegimentTargetData EnemyRegimentTargetData => LinkedRegiment.EnemyRegimentTargetData;
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
