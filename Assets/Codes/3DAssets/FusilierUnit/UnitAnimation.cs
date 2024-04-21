using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RandomUnity = UnityEngine.Random;
using Random = Unity.Mathematics.Random;

namespace Kaizerwald
{
    public enum EAnimationParams
    {
        // Animation Speed
        GlobalAnimationSpeed,
        // Move
        Velocity,
        // Fire
        IsAiming,
        IsShooting,
        // Death
        TriggerDeath,
        DeathIndex
    }
    
    public class UnitAnimation : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [SerializeField] private ParticleSystem RifleParticleSystem;
        
        [SerializeField] private bool IsShooting;
        [SerializeField] private bool IsAiming;
        [SerializeField] private bool IsDead; 
        [SerializeField] private bool IsInMelee;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public Animator Animator { get; private set; }
        public Dictionary<EAnimationParams, int> AnimationParameters { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public bool IsInAimingMode => IsAiming;
        public bool IsInFiringMode => IsAiming && IsShooting;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Events ◈◈◈◈◈◈                                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public event Action<AnimationEvent> OnShootEvent;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void Awake()
        {
            Animator = GetComponent<Animator>();
            RifleParticleSystem = RifleParticleSystem == null ? GetComponentInChildren<ParticleSystem>() : RifleParticleSystem;
            
            AssignAnimationIDs();
            Animator.SetFloat(AnimationParameters[EAnimationParams.GlobalAnimationSpeed], RandomUnity.Range(0.9f, 1f));
            Animator.SetInteger(AnimationParameters[EAnimationParams.DeathIndex], RandomUnity.Range(0, 3));
        }
        
        private void Update()
        {
            ToggleFireAnimation();
            ToggleDeath();
        }
        
#if UNITY_EDITOR
        private void ToggleFireAnimation()
        {
            if (!Keyboard.current.eKey.wasPressedThisFrame) return;
            SetFullFireSequence(!IsInFiringMode);
        }
        
        private void ToggleDeath()
        {
            if (!Keyboard.current.kKey.wasPressedThisFrame) return;
            SetDead(!IsDead);
        }
#endif
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Current State Accessor ◈◈◈◈◈◈                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private string CurrentClipName => Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        public bool IsPlayingIdle => CurrentClipName == "RifleIdle";
        public bool IsPlayingFire => CurrentClipName == "RifleFiring";
        public bool IsPlayingReload => CurrentClipName == "RifleReloading";
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Current State Accessor ◈◈◈◈◈◈                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void AssignAnimationIDs()
        {
            AnimationParameters = new Dictionary<EAnimationParams, int>
            {
                // Animation Speed
                { EAnimationParams.GlobalAnimationSpeed, Animator.StringToHash("GlobalAnimationSpeed") },
                
                // Move
                { EAnimationParams.Velocity, Animator.StringToHash("Velocity") },
                
                // Fire
                { EAnimationParams.IsAiming, Animator.StringToHash("IsAiming") },
                { EAnimationParams.IsShooting, Animator.StringToHash("IsShooting") },
                
                // Death
                { EAnimationParams.TriggerDeath, Animator.StringToHash("TriggerDeath") },
                { EAnimationParams.DeathIndex, Animator.StringToHash("DeathIndex") }
            };
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Animation Triggers ◈◈◈◈◈◈                                                                               ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Move ◇◇◇◇◇◇                                                                                        │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public void SetVelocity(float value)
        {
            Animator.SetFloat(AnimationParameters[EAnimationParams.Velocity], value);
        }

        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Aim | Fire ◇◇◇◇◇◇                                                                                  │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public void SetIsAiming(bool enable)
        {
            if (enable && IsInMelee) return;
            Animator.SetBool(AnimationParameters[EAnimationParams.IsAiming], IsAiming = enable);
        }
        
        public void SetIsShooting(bool enable)
        {
            if (enable && IsInMelee) return;
            Animator.SetBool(AnimationParameters[EAnimationParams.IsShooting], IsShooting = enable);
        }
        
        public void SetFullFireSequence(bool enable)
        {
            if (enable && IsInMelee) return;
            Animator.SetBool(AnimationParameters[EAnimationParams.IsAiming], IsAiming = enable);
            Animator.SetBool(AnimationParameters[EAnimationParams.IsShooting], IsShooting = enable);
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Dead ◇◇◇◇◇◇                                                                                        │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

        public void TriggerDeath()
        {
            if (IsDead) return;
            Animator.SetTrigger(AnimationParameters[EAnimationParams.TriggerDeath]);
            IsDead = true;
        }
        
        public void SetDead(bool enable = true)
        {
            if (enable == IsDead) return;
            if (enable)
            {
                Animator.SetTrigger(AnimationParameters[EAnimationParams.TriggerDeath]);
            }
            else
            {
                Animator.ResetTrigger(AnimationParameters[EAnimationParams.TriggerDeath]);
                Animator.SetInteger(AnimationParameters[EAnimationParams.DeathIndex], -1);
                Animator.Play("Idle");
            }
            IsDead = enable;
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Animation Events ◈◈◈◈◈◈                                                                                 ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void OnShootTrigger(AnimationEvent animationEvent)
        {
            OnShootEvent?.Invoke(animationEvent);
            RifleParticleSystem.Play();
        }
    }
}
