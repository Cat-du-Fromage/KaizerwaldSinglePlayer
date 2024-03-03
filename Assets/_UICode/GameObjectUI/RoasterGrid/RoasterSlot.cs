using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kaizerwald
{
    public sealed class RoasterSlot : BaseSlot
    {
        [SerializeField] private GameObject ArmyItemPrefabRelated;
        private RoasterItem RoasterItemReference
        {
            get
            {
                if (CardReference == null) return null;
                return (RoasterItem)CardReference;
            }
        }
        
        public override void Initialize(CardDetails cardDetails)
        {
            base.Initialize(cardDetails);
        }
    }
}
