/*
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KaizerWald
{
    
    public partial class UnitAnimation : MonoBehaviour
    {
        public bool DebugAnimationActive = false;
        
        private void OnGUI()
        {
            TestAnimationPlaying();
        }

        private void TestAnimationPlaying()
        {
            if (!DebugAnimationActive) return;
            if (!Keyboard.current.qKey.wasPressedThisFrame) return;
            Debug.Log($"CurrentClipName: {GetCurrentClipName}");
            Debug.Log($"IsIdle?: {IsPlaying(EUnitAnimation.RifleIdle)}; IsFiring?: {IsPlaying(EUnitAnimation.FusilierFiring)}");
        }
    }
    
}
*/