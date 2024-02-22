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
        
        protected const float REACH_DISTANCE_THRESHOLD = 0.0125f;
        
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
        //protected RegimentType RegimentType => RegimentStateReference.RegimentType;
        //protected EnemyRegimentTargetData EnemyRegimentTargetData => RegimentStateReference.EnemyRegimentTargetData;
    
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
        /*
        protected bool ShouldRearrange()
        {
            float3 targetPosition = LinkedParentRegiment.TargetPosition;
            FormationData formation = LinkedParentRegiment.TargetFormation;
            float3 positionInFormation = formation.GetUnitRelativePositionToRegiment3D(IndexInFormation, targetPosition);
            return math.distancesq(Position.xz, positionInFormation.xz) > REACH_DISTANCE_THRESHOLD;
        }
        */
        protected abstract EStates TryReturnToRegimentState();
    }
}
