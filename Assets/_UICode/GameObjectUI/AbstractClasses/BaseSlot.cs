using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    public abstract class BaseSlot : MonoBehaviour
    {
        [field:SerializeField] public GameObject AttachedCard { get; protected set; }
        [field:SerializeField] public BaseItem CardReference { get; protected set; }
        [field:SerializeField] public CardDetails CardDetails { get; protected set; }
        
        protected Transform SlotTransform;

        protected virtual void Awake()
        {
            SlotTransform = transform;
        }
        
        public virtual void Initialize(CardDetails cardDetails)
        {
            CardDetails = cardDetails;
            if (transform.childCount > 0)
            {
                AttachedCard = transform.GetChild(0).gameObject;
                CardReference = AttachedCard.GetComponent<BaseItem>().Initialize(cardDetails);
            }
        }
        
        public virtual void AddItem<T1>(GameObject slotItemPrefab, CardDetails cardDetails)
        where T1 : BaseItem
        {
            GameObject item = Instantiate(slotItemPrefab, transform);
            T1 itemComponent = item.GetComponent<T1>();
            itemComponent.Initialize(cardDetails);
            Initialize(cardDetails);
        }
    }
}
