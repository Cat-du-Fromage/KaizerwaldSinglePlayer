using System;
using System.Collections;
using System.Collections.Generic;
using Kaizerwald.FormationModule;
using UnityEngine;

namespace Kaizerwald
{
    public sealed class HighlightUnit : FormationElementBehaviour
    {
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [field: SerializeField] public bool IsActive { get; private set; } = true;
        [field: SerializeField] public HighlightRegiment HighlightRegimentAttach { get; private set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void OnDestroy()
        {
            //HighlightRegimentAttach.RemoveUnit(this);
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void SetActive() => IsActive = true;
        public void SetInactive() => IsActive = false;
        
        public void AttachToRegiment(HighlightRegiment regimentToAttach)
        {
            HighlightRegimentAttach = regimentToAttach;
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ IFormationElement ◈◈◈◈◈◈                                                                                ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public void TriggerDeath()
        {
            //HighlightRegimentAttach.Remove(this);
            HighlightRegimentAttach.RegisterInactiveElement(this);
        }
    }
}
