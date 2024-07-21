using System;
using System.Collections;
using System.Collections.Generic;
using Kaizerwald.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using Kaizerwald.Pattern;

namespace Kaizerwald
{
    public enum EAbilityType
    {
        MarchRun,
        AutoFire,
    }
    
    public readonly struct AbilityTrigger
    {
        public readonly int RegimentID;
        public readonly EAbilityType AbilityType;

        public AbilityTrigger(int regimentID, EAbilityType abilityType)
        {
            RegimentID = regimentID;
            AbilityType = abilityType;
        }
    }
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HighlightRegimentManager))]
    public class HighlightAbilityController : Singleton<HighlightAbilityController>, PlayerControls.IRegimentAbilityActions
    {
        private PlayerControls controls;
        private HighlightRegimentManager highlightManager;

        public List<HighlightRegiment> Selections => highlightManager.SelectedRegiments;
        private bool SelectedRegimentsEmpty => highlightManager.SelectedRegiments.Count == 0;
        
        public event Action<AbilityTrigger[]> OnAbilityTriggered;

        protected override void OnAwake()
        {
            base.OnAwake();
            controls ??= new PlayerControls();
            if (!controls.RegimentAbility.enabled)
            {
                controls.RegimentAbility.Enable();
            }
            controls.RegimentAbility.SetCallbacks(this);
            highlightManager = GetComponent<HighlightRegimentManager>();
        }

        private void AbilityCallback(EAbilityType abilityType)
        {
            AbilityTrigger[] abilitiesTriggered = new AbilityTrigger[Selections.Count];
            for (int i = 0; i < Selections.Count; i++)
            {
                abilitiesTriggered[i] = new AbilityTrigger(Selections[i].RegimentID, abilityType);
            }
            OnAbilityTriggered?.Invoke(abilitiesTriggered);
        }

        public void OnMarchRun(InputAction.CallbackContext context)
        {
            if (SelectedRegimentsEmpty || !context.performed) return;
            AbilityCallback(EAbilityType.MarchRun);
        }

        public void OnAutoFire(InputAction.CallbackContext context)
        {
            if (SelectedRegimentsEmpty || !context.performed) return;
            AbilityCallback(EAbilityType.AutoFire);
        }
    }
}
