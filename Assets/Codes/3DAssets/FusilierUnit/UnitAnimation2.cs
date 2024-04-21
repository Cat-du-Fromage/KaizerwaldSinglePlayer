using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

using static UnityEngine.Animator;
using static Unity.Mathematics.math;
using RandomUnity = UnityEngine.Random;
using Random = Unity.Mathematics.Random;

using RotaryHeart.Lib.SerializableDictionary;

namespace Kaizerwald
{
    [Serializable]
    public class AnimationNamePairClip : SerializableDictionaryBase<string, AnimationClip> { }
    
    //TODO: FIND BETTER SOLUTION...
    public enum EUnitAnimation
    {
        RifleIdle,
        WalkRifle,
        RunRifle,
        RifleDownToAim,
        RifleAimingIdle,
        RifleAimToDown,
        FusilierFiring,
        RifleReloading,
        RifleFrontDeath0,
        RifleFrontDeath1,
        RifleFrontDeath2,
    }
    
    public class UnitAnimation2 : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [SerializeField] private ParticleSystem MuzzleFlash;
        [SerializeField] private float animationsSpeed;
        //[SerializeField] private float speedIdle;
        
        private Animator animator;
        
        //can aim but stay Idle!
        public bool shoot, aim;
        //trigger
        private int animTriggerIDDeath;
        //int: choose which animation to play based on Id
        private int animIDDeathIndex;
        //Speeds
        private int animIDAnimationsSpeed, animIDSpeed;//, animIDIdleSpeed;
        //bool: enable/disable animation
        private int animIDIsAiming, animIDIsShooting;
        
        public event Action<AnimationEvent> OnShootEvent;
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private AnimationNamePairClip AnimationClips;

        public bool IsInAimingMode => aim;
        public bool IsInFiringMode => aim && shoot;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        private void Awake()
        {
            InitializeComponents();
            AssignAnimationIDs();
        }

        private void Start()
        {
            //InitializeIdleRandom(GetInstanceID());
            InitializeIdleRandom();
            GetAllCLips();
        }
        
        private void Update()
        {
            ToggleFireAnimation();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void ToggleFireAnimation()
        {
            if (!Keyboard.current.eKey.wasPressedThisFrame) return;
            SetFullFireSequence(!IsInFiringMode);
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Current State Accessor ◈◈◈◈◈◈                                                                       ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private string GetCurrentClipName => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        
        public bool IsPlaying(EUnitAnimation unitAnimation) => GetCurrentClipName == unitAnimation.ToString();
        public bool IsPlayingIdle   => GetCurrentClipName == nameof(EUnitAnimation.RifleIdle);
        public bool IsPlayingFire   => GetCurrentClipName == nameof(EUnitAnimation.FusilierFiring);
        public bool IsPlayingReload => GetCurrentClipName == nameof(EUnitAnimation.RifleReloading);

        //TODO: Since unit share all the same animation, We need to Moveit to UnitManager (at least the way to retrieve them)
        private void GetAllCLips()
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            AnimationClips = new AnimationNamePairClip();
            foreach (AnimationClip clip in clips)
            {
                AnimationClips.Add(clip.name, clip);
            }
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        private void InitializeComponents()
        {
            animator = GetComponent<Animator>();
            if (MuzzleFlash == null) TryGetComponent(out MuzzleFlash);
            MuzzleFlash = MuzzleFlash == null ? GetComponentInChildren<ParticleSystem>() : MuzzleFlash;
        }
        
        private void AssignAnimationIDs()
        {
            animTriggerIDDeath    = StringToHash("TriggerDeath");
            animIDDeathIndex      = StringToHash("DeathIndex");
            animIDAnimationsSpeed = StringToHash("GlobalAnimationSpeed");
            animIDSpeed           = StringToHash("Velocity");
            animIDIsAiming        = StringToHash("IsAiming");
            animIDIsShooting      = StringToHash("IsShooting");
            //animIDIdleSpeed       = StringToHash("IdleSpeed");
        }
        
        public void InitializeIdleRandom(int index)
        {
            Random rand = Random.CreateFromIndex(min((uint)abs(index), uint.MaxValue - 1));
            //speedIdle = rand.NextFloat(4, 11) / 10f;
            //animator.SetFloat(animIDIdleSpeed, speedIdle);
            animationsSpeed = rand.NextFloat(6, 11) / 10f;
            animator.SetFloat(animIDAnimationsSpeed, animationsSpeed);
            int randomDeathIndex = rand.NextInt(0, 3);
            animator.SetInteger(animIDDeathIndex, randomDeathIndex); //Random.CreateFromIndex((uint)index).NextInt(0, 3);
        }
        
        public void InitializeIdleRandom()
        {
            //speedIdle = RandomUnity.Range(0.5f, 1f);
            //animator.SetFloat(animIDIdleSpeed, speedIdle);
            animationsSpeed = RandomUnity.Range(0.9f, 1f);
            animator.SetFloat(animIDAnimationsSpeed, animationsSpeed);
            //Range(int) is exclusive contrary to Range(float)
            animator.SetInteger(animIDDeathIndex, RandomUnity.Range(0, 3));
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Animation Triggers ◈◈◈◈◈◈                                                                               ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ March/Run ◇◇◇◇◇◇                                                                                   │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public void SetSpeed(float value)
        {
            animator.SetFloat(animIDSpeed, value);
        }
        public void SetIdle() => SetSpeed(0);
        public void SetMarching() => SetSpeed(2);
        public void SetTrotting() => SetSpeed(4);
        public void SetRunning() => SetSpeed(6);
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Aim ◇◇◇◇◇◇                                                                                         │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public void SetAimOn() => animator.SetBool(animIDIsAiming, aim = true);
        public void SetAimOff() => animator.SetBool(animIDIsAiming, aim = false);
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Fire ◇◇◇◇◇◇                                                                                        │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public void SetFireOn() => animator.SetBool(animIDIsShooting, shoot = true);
        public void SetFireOff() => animator.SetBool(animIDIsShooting, shoot = false);
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Aim + FIre ◇◇◇◇◇◇                                                                                  │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void SetFullFireSequence(bool state)
        {
            if (state) SetSpeed(0);
            animator.SetBool(animIDIsAiming, aim = state);//aim = state;
            animator.SetBool(animIDIsShooting, shoot = state);//shoot = state;
        }
        public void SetFullFireSequenceOn() => SetFullFireSequence(true);
        public void SetFullFireSequenceOff() => SetFullFireSequence(false);

        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Dead ◇◇◇◇◇◇                                                                                        │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public void SetDead()
        {
            animator.SetTrigger(animTriggerIDDeath);
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Animation Events ◈◈◈◈◈◈                                                                                 ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void OnShootTrigger(AnimationEvent animationEvent)
        {
            OnShootEvent?.Invoke(animationEvent);
            MuzzleFlash.Play();
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Test To Get Time ◇◇◇◇◇◇                                                                            │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        /*
        private float GetCurrentAnimatorTime()
        {
            AnimatorStateInfo animationState = animator.GetCurrentAnimatorStateInfo(0);
            AnimatorClipInfo[] myAnimatorClip = animator.GetCurrentAnimatorClipInfo(0);
            float myTime = myAnimatorClip[0].clip.length * animationState.normalizedTime;
            return myTime;
        }
        */
    }
}