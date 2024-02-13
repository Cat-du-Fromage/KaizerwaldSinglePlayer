using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering.Universal;

namespace Kaizerwald
{
    public sealed class SelectionBehaviour : HighlightBehaviour
    {
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private PositionConstraint positionConstraint;
        private DecalProjector projector;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void Awake()
        {
            projector = GetComponent<DecalProjector>();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public override bool IsVisible() => projector.enabled == true;
        public override bool IsHidden() => projector.enabled == false;
        
        public override void InitializeHighlight(GameObject unitAttached)
        {
            positionConstraint = GetComponent<PositionConstraint>();
            AttachToUnit(unitAttached);
            Hide();
        }
        
        public override void AttachToUnit(GameObject unit)
        {
            base.AttachToUnit(unit);
            UnlockConstraint();
            transform.position = UnitTransform.position + Vector3.up;

            if (positionConstraint.sourceCount == 0)
            {
                positionConstraint.AddSource(new ConstraintSource { sourceTransform = UnitTransform , weight = 1});
            }
            else
            {
                positionConstraint.SetSource(0, new ConstraintSource { sourceTransform = UnitTransform , weight = 1});
            }
            
            positionConstraint.translationAxis = Axis.X | Axis.Y | Axis.Z;
            LockConstraint();
        }

        private void LockConstraint()
        {
            positionConstraint.constraintActive = true;
            positionConstraint.locked = true;
        }
        
        private void UnlockConstraint()
        {
            positionConstraint.constraintActive = false;
            positionConstraint.locked = false;
        }
        
        public override void Show()
        {
            positionConstraint.translationAxis = Axis.X | Axis.Y | Axis.Z;
            transform.position = UnitTransform.position + Vector3.up;
            positionConstraint.locked = true;
            projector.enabled = true;
        }

        public override void Hide()
        {
            positionConstraint.locked = false;
            positionConstraint.translationAxis = Axis.None;
            projector.enabled = false;
        }
    }
}