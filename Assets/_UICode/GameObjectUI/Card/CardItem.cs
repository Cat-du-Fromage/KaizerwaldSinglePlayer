using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    [Serializable]
    public class CardItem
    {
        public CardDetails CardDetails;
        public RegimentType RegimentType;
        [field:SerializeField] public SerializableGuid UniqueId { get; private set; }
        [field:SerializeField] public SerializableGuid DetailsId { get; private set; }
        
        public CardItem(CardDetails details) 
        {
            CardDetails = details;
            RegimentType = details.RegimentType;
            UniqueId = SerializableGuid.NewGuid();
            DetailsId = details.Id;
        }
    }
}
