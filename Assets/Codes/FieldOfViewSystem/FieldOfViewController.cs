using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

using static Unity.Mathematics.math;

namespace Kaizerwald.FieldOfView
{
    [DisallowMultipleComponent]
    public partial class FieldOfViewController : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        private Transform cachedTransform;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [field:SerializeField] public FieldOfView FieldOfView { get; private set; }
        [field:SerializeField] public FieldOfViewParams FovParams { get; private set; }

        public float3 Position => cachedTransform.position;
        public float3 Forward => cachedTransform.forward;
        
        public FieldOfViewBounds Bounds => new FieldOfViewBounds(FovParams, Position, Forward);
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        /*
        private void Awake()
        {
            BaseInitialize();
        }
        */
        private void OnDestroy()
        {
            FieldOfViewManager.Instance.UnRegister(this);
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Update Manager Events ◈◈◈◈◈◈                                                                            ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void OnFixedUpdate()
        {
            FieldOfView.OnFixedUpdate();
        }
        
        public void OnLateUpdate()
        {
            FieldOfView.OnLateUpdate();
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                           ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Player Controls ◈◈◈◈◈◈                                                                                  ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        //Link to Highlight/Selection system or using C# event (=> add event OnSelected in highlight system)?
        public void EnableFov()
        {
            FieldOfView.Show();
        }

        //Link to Highlight/Selection system or using C# event (=> add event OnSelected in highlight system)?
        public void DisableFov()
        {
            FieldOfView.Hide();
        }

        public void OnWidthChange()
        {
            //Update Mesh
            //Need C# event from Formation or Regiment?
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Initialization ◈◈◈◈◈◈                                                                                   ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private bool BaseInitialize()
        {
            if (FieldOfView != null) return true;
            if (FieldOfViewManager.Instance.FovPrefab == null)
            {
                print("NO Field Of View Prefab Referenced !");
                return false;
            }
            if (transform.childCount == 0 || (FieldOfView = transform.GetComponentInChildren<FieldOfView>()) == null)
            {
                FieldOfView = Instantiate(FieldOfViewManager.Instance.FovPrefab, transform).GetComponent<FieldOfView>();
            }
            return true;
        }
        
        public FieldOfViewController Initialize(float range, float sideAngleDegrees, float widthLength, Vector3 positionOffset = default)
        {
            if (!BaseInitialize()) return this;
            cachedTransform = transform;
            FovParams = new FieldOfViewParams(range, radians(sideAngleDegrees), widthLength);
            
            FieldOfView.Initialize(this, 1f);
            FieldOfView.transform.localPosition = Vector3.zero + positionOffset;
            
            FieldOfViewManager.Instance.Register(this);
            DisableFov();
            return this;
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Detection Method ◈◈◈◈◈◈                                                                                 ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public bool IsInsideFieldOfView(float3 point)
        {
            return Bounds.IsPointInsideFov(point);
        }
    }
    
    
}