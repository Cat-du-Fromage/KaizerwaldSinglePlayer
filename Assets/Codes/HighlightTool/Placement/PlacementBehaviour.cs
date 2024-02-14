using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    public sealed class PlacementBehaviour : HighlightBehaviour
    {
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private MeshRenderer meshRenderer;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            Hide();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public override bool IsVisible() => meshRenderer.enabled == true;
        public override bool IsHidden() => meshRenderer.enabled == false;

        public override void Show() => meshRenderer.enabled = true;
        public override void Hide() => meshRenderer.enabled = false;
        
        public override void InitializeHighlight(GameObject unitAttached)
        {
            LinkToUnit(unitAttached);
            meshRenderer = GetComponent<MeshRenderer>();
            Vector3 position = UnitTransform.position + Vector3.up * 0.05f;
            transform.SetPositionAndRotation(position, UnitTransform.rotation);
            Hide();
        }
    }
}
