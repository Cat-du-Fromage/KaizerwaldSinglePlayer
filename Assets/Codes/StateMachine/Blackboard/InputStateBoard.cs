using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    public class InputStateBoard
    {
        public bool AutoFire { get; private set; } = true;
        public bool Run { get; private set; }
        public bool IsInMeleeMode { get; private set; } = true;

        public void SetInput(EAbilityType abilityType)
        {
            switch (abilityType)
            {
                case EAbilityType.MarchRun:
                    Run = !Run;
                    //Debug.Log($"SetInput Run = {Run}");
                    break;
                case EAbilityType.AutoFire:
                    AutoFire = !AutoFire;
                    break;
                default:
                    break;
            }
        }

        public void SetRun(bool enable) => Run = enable;
    }
}
