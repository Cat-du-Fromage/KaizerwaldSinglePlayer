using System;
using System.Collections;
using System.Collections.Generic;
using Kaizerwald.Pattern;
using Kaizerwald.Utilities.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Kaizerwald.FieldOfView
{
    public class FieldOfViewManager : SingletonBehaviour<FieldOfViewManager>
    {
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                               ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                  ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [SerializeField] private Material FovMaterial;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [field:SerializeField] public GameObject FovPrefab { get; private set; }
        [field:SerializeField] public LayerMask TerrainLayer { get; private set; }

        [field:SerializeField] public List<FieldOfViewController> FieldOfViewControllers { get; private set; } = new();
        [field:SerializeField] public List<FieldOfView> FieldOfViews { get; private set; } = new();
        
        public Dictionary<int, FieldOfViewController> FieldOfViewByRegimentID { get; private set; } = new ();
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void FixedUpdate()
        {
            for (int i = 0; i < FieldOfViews.Count; i++)
            {
                FieldOfViewControllers[i].OnFixedUpdate();
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < FieldOfViews.Count; i++)
            {
                FieldOfViewControllers[i].OnLateUpdate();
            }
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                           ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void Register(FieldOfViewController newFov)
        {
            FieldOfViewControllers.AddUnique(newFov);
            FieldOfViewByRegimentID.TryAdd(newFov.gameObject.GetInstanceID(), newFov);
            
            FieldOfViews.AddUnique(newFov.FieldOfView);
        }
        
        public void UnRegister(FieldOfViewController deletedFov)
        {
            FieldOfViews.Remove(deletedFov.FieldOfView);
            FieldOfViewByRegimentID.Remove(deletedFov.gameObject.GetInstanceID());
            
            FieldOfViewControllers.Remove(deletedFov);
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ External Events ◈◈◈◈◈◈                                                                                  ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public void OnShowFieldOfView(int fovObjectId)
        {
            FieldOfViewByRegimentID[fovObjectId].EnableFov();
        }
        
        public void OnHideFieldOfView(int fovObjectId)
        {
            FieldOfViewByRegimentID[fovObjectId].DisableFov();
        }
    }
}
