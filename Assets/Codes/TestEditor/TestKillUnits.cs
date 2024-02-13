#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaizerwald
{
    public class TestKillUnits : MonoBehaviour
    {
        [SerializeField] private LayerMask UnitLayer;
        [SerializeField] private Camera PlayerCamera;
        
        private void Awake()
        {
            if(PlayerCamera == null) PlayerCamera = Camera.main;
        }

        private void Update()
        {
            if (UnitLayer == default) return;
            TestKillUnit();
        }

        private void TestKillUnit()
        {
            if (!Mouse.current.rightButton.wasPressedThisFrame) return;
            Ray singleRay = PlayerCamera.ScreenPointToRay(Mouse.current.position.value);
            if (!Physics.Raycast(singleRay, out RaycastHit hit, 1000, UnitLayer)) return;
            Unit unit = hit.transform.GetComponent<Unit>();
            if (unit.IsInactive) return;
            unit.TriggerDeath();
        }
    }
}
#endif
