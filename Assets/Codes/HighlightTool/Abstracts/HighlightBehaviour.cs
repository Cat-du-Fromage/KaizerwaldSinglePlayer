using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

namespace Kaizerwald
{
    public abstract class HighlightBehaviour : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [field:SerializeField] public GameObject LinkedUnit { get; protected set; }
        protected Transform UnitTransform => LinkedUnit.transform;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public abstract void InitializeHighlight(GameObject unitAttached);
        public virtual void LinkToUnit(GameObject unit) => LinkedUnit = unit;
        
        public abstract void Show();
        public abstract void Hide();
        public abstract bool IsVisible();
        public abstract bool IsHidden();
    }
}