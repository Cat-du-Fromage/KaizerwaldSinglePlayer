using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering.Universal;

namespace Kaizerwald
{
    public sealed class PreselectionBehaviour : HighlightBehaviour
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
            positionConstraint.translationAxis = Axis.X | Axis.Y | Axis.Z;
            ConstraintSource source = new ConstraintSource { sourceTransform = UnitTransform, weight = 1 };
            if (positionConstraint.sourceCount == 0)
            {
                positionConstraint.AddSource(source);
            }
            else
            {
                positionConstraint.SetSource(0, source);
            }
            transform.position = UnitTransform.position + Vector3.up;
            LockConstraint();
        }

        private void LockConstraint()
        {
            positionConstraint.constraintActive = positionConstraint.locked = true;
        }
        
        private void UnlockConstraint()
        {
            positionConstraint.constraintActive = positionConstraint.locked = false;
        }

        public override void Show()
        {
            positionConstraint.translationAxis = Axis.X | Axis.Y | Axis.Z;
            transform.position = UnitTransform.position + Vector3.up;
            LockConstraint();
            projector.enabled = true;
        }

        public override void Hide()
        {
            projector.enabled = false;
            UnlockConstraint();
            positionConstraint.translationAxis = Axis.None;
        }
    }
}