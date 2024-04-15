using System.Collections;
using System.Collections.Generic;
using Kaizerwald.FormationModule;
using UnityEngine;
using Unity.Mathematics;

namespace Kaizerwald.StateMachine
{
    public abstract class UnitStateBase<T> : StateBase<UnitStateMachine>
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
        protected RegimentStateMachine LinkedRegimentStateMachine => StateMachine.RegimentStateMachine;
        protected EStates RegimentState => StateMachine.RegimentState;
        protected bool IsRegimentStateIdentical => StateIdentity == RegimentState;
    
        // Regiment
        protected Regiment LinkedParentRegiment => RegimentStateReference.LinkedRegiment;
        protected Formation TargetFormation => RegimentStateReference.TargetFormation;
    
        // Unit
        protected Unit LinkedUnit => StateMachine.LinkedUnit;
        protected Transform UnitTransform => StateMachine.CachedTransform;
        protected UnitAnimation UnitAnimation => StateMachine.LinkedUnit.Animation;
        protected int IndexInFormation => StateMachine.LinkedUnit.IndexInFormation;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected UnitStateBase(UnitStateMachine stateMachine, EStates stateIdentity) : base(stateMachine, stateIdentity)
        {
            RegimentStateReference = (T)stateMachine.RegimentStateMachine.States[stateIdentity];
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        protected virtual bool TryReturnToRegimentState(out EStates nextState)
        {
            bool canEnterNextState = StateMachine.States[RegimentState].ConditionEnter();
            nextState = canEnterNextState ? RegimentState : DefaultNextState;
            return canEnterNextState;
        }
    }
}
