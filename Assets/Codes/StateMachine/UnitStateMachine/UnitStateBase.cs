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
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        protected T RegimentStateReference { get; private set; }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        // Regiment
        protected Regiment LinkedParentRegiment => RegimentStateReference.LinkedRegiment;
        protected RegimentBehaviourTree LinkedRegimentBehaviourTree => BehaviourTree.RegimentBehaviourTree;
        
        protected EStates RegimentState => BehaviourTree.RegimentState;
        protected bool IsRegimentStateIdentical => StateIdentity == RegimentState;
    
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
        protected abstract EStates TryReturnToRegimentState();
    }
}
