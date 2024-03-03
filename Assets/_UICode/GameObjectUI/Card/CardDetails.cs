using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    [CreateAssetMenu(fileName = "NewCardItemDetails", menuName = "SelectionMenu/CardItemDetails")]
    [Serializable]
    public class CardDetails : ScriptableObject
    {
        public string Name;
        public Sprite Icon;
        //Add Regiment Type
        public SerializableGuid Id = SerializableGuid.NewGuid();
        
        private void AssignNewGuid()
        {
            Id = SerializableGuid.NewGuid();
        }
        
        public static implicit operator CardItem(CardDetails details) => new CardItem(details);
    }
}
