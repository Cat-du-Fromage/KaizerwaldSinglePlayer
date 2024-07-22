#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Kaizerwald.Pattern;
using Kaizerwald.Utilities.Core;

namespace Kaizerwald.TerrainBuilder
{
    public partial class SimpleTerrain : SingletonBehaviour<SimpleTerrain>
    {
        [field:SerializeField] public bool AutoUpdate { get; private set; }
        
        public void DrawMapInEditor()
        {
            Initialize();
        }
        
        /*
        private void OnDrawGizmos()
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, TerrainLayerMask)) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hit.point, 0.45f);
        }
        */
    }
}
#endif