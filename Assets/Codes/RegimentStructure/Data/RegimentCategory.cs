using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    [CreateAssetMenu(fileName = "NewRegimentCategory", menuName = "Regiment/Category", order = 0)]
    public class RegimentCategory : ScriptableObject
    {
        [Header("Prefabs")]
        public GameObject PlacementPrefab;
        
        [Header("Stats")]
        public Vector3 UnitSize;
        
        private void OnEnable()
        {
            if (PlacementPrefab == null) return;
            UnitSize = Vector3.one;
            //UnitSize = PlacementPrefab.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        }
    }
}
