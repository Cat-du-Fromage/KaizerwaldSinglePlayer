#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using static UnityEngine.Physics;

namespace Kaizerwald
{
    public class TestKillUnits : MonoBehaviour
    {
        [SerializeField] private LayerMask UnitLayer;
        [SerializeField] private Camera PlayerCamera;

        private RaycastHit[] raycastHits = new RaycastHit[1];
        
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
            if (!Keyboard.current.rKey.wasPressedThisFrame) return;
            Ray singleRay = PlayerCamera.ScreenPointToRay(Mouse.current.position.value);
            if (SphereCastNonAlloc(singleRay, 0.25f, raycastHits, 1024, UnitLayer) == 0) return;
            //if (!Raycast(singleRay, out RaycastHit hit, 1000, UnitLayer)) return;
            Unit unit = raycastHits[0].transform.GetComponent<Unit>();
            if (unit.IsInactive) return;
            unit.TriggerDeath();
        }
    }
}
#endif
