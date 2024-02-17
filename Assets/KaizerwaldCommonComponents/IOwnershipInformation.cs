using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    public interface IOwnershipInformation
    {
        public ulong OwnerPlayerID { get; }
        public int TeamID { get; }
    }
}
