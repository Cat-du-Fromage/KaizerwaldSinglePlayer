using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Kaizerwald.Utilities;

namespace Kaizerwald
{
    public class KaizerwaldGameManager : Singleton<KaizerwaldGameManager>
    {
        [field:SerializeField] public LayerMask TerrainLayerMask { get; private set; }
        [field:SerializeField] public LayerMask UnitLayerMask { get; private set; }

        public int TerrainLayerIndex => TerrainLayerMask.GetLayerIndex();
        public int UnitLayerIndex => UnitLayerMask.GetLayerIndex();
        
        protected override void OnAwake()
        {
            base.OnAwake();
        }
    }
}
