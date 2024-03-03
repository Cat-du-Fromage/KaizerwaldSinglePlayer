using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Kaizerwald
{
    public abstract class BaseItem : MonoBehaviour
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected Transform ItemTransform;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [field:SerializeField] public CardItem CardItem { get; protected set; }
        [field:SerializeField] public Image ItemRaycastImage { get; protected set; }
        [field:SerializeField] public Transform ParentAfterDrag { get; protected set; }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENT ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void Awake()
        {
            ItemTransform = transform;
            ItemRaycastImage = GetComponent<Image>();
        }
        
        protected bool ShouldBeDestroyed()
        {
            bool shouldBeDestroyed = !ParentAfterDrag.TryGetComponent(out ArmySlot _);
            //Debug.Log($"ShouldBeDestroyed: RoasterSlot ? {!ParentAfterDrag.TryGetComponent(out ArmySlot _)}");
            if (shouldBeDestroyed)
            {
                //Debug.Log($"OnPointerUp: Destroy");
                DestroyImmediate(gameObject);
            }
            return shouldBeDestroyed;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        public BaseItem Initialize(CardDetails details)
        {
            ParentAfterDrag = ItemTransform.parent;
            CardItem = details;
            ItemRaycastImage.sprite = details.Icon;
            return this;
        }
        
        public void SetParentAfterDrag(Transform parentAfterDrag)
        {
            ParentAfterDrag = parentAfterDrag;
        }
    }
}
