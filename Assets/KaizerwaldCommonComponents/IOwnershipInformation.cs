using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    public interface IOwnershipInformation
    {
        public ulong OwnerPlayerID { get; }
        public short TeamID { get; }
    }

    [Serializable]
    public struct PlayerInfos : IOwnershipInformation, IEquatable<PlayerInfos>
    {
        [field:SerializeField] public ulong OwnerPlayerID { get; private set; }
        [field:SerializeField] public short TeamID { get; private set; }

        public PlayerInfos(ulong playerId = 0, int teamID = 0)
        {
            OwnerPlayerID = playerId;
            TeamID = (short)teamID;
        }

        public bool Equals(PlayerInfos other)
        {
            return OwnerPlayerID == other.OwnerPlayerID;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerInfos other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(OwnerPlayerID);
        }
    }
}
