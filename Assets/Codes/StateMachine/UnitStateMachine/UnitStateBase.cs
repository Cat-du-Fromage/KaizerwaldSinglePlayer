using System.Collections;
using System.Collections.Generic;
using Kaizerwald.FormationModule;
using UnityEngine;
using Unity.Mathematics;

namespace Kaizerwald.StateMachine
{
    public abstract class UnitStateBase<T> : StateBase<UnitBehaviourTree>
    where T : RegimentStateBase
    {
        protected const EStates DefaultNextState = EStates.Idle;
        
        protected const float REACH_DISTANCE_THRESHOLD = 0.0125f; //was 0.0125f
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected T RegimentStateReference { get; private set; }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        protected RegimentBehaviourTree LinkedRegimentBehaviourTree => BehaviourTree.RegimentBehaviourTree;
        protected EStates RegimentState => BehaviourTree.RegimentState;
        protected bool IsRegimentStateIdentical => StateIdentity == RegimentState;
    
        // Regiment
        protected Regiment LinkedParentRegiment => RegimentStateReference.LinkedRegiment;
        protected Formation TargetFormation => RegimentStateReference.TargetFormation;
    
        // Unit
        protected Unit LinkedUnit => BehaviourTree.LinkedUnit;
        protected Transform UnitTransform => BehaviourTree.CachedTransform;
        protected UnitAnimation UnitAnimation => BehaviourTree.LinkedUnit.Animation;
        protected int IndexInFormation => BehaviourTree.LinkedUnit.IndexInFormation;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected UnitStateBase(UnitBehaviourTree behaviourTree, EStates stateIdentity) : base(behaviourTree, stateIdentity)
        {
            RegimentStateReference = (T)behaviourTree.RegimentBehaviourTree.States[stateIdentity];
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        protected virtual bool TryReturnToRegimentState(out EStates nextState)
        {
            bool canEnterNextState = BehaviourTree.States[RegimentState].ConditionEnter();
            nextState = canEnterNextState ? RegimentState : DefaultNextState;
            return canEnterNextState;
        }
    }
}
