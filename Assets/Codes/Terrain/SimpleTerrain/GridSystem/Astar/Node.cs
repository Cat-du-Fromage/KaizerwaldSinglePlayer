using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Kaizerwald.Utilities;
using Unity.VisualScripting;
using static Kaizerwald.Utilities.KzwMath;

namespace Kaizerwald.TerrainBuilder
{
    public readonly struct Node
    {
        public readonly int CameFromNodeIndex;
        public readonly int GCost; // Distance from Start Node
        public readonly int HCost; // distance from End Node
        public readonly int FCost;
        public readonly int2 Coords;

        public Node(int cameFromNodeIndex, int gCost, int hCost, int2 coords)
        {
            CameFromNodeIndex = cameFromNodeIndex;
            GCost = gCost;
            HCost = hCost;
            FCost = GCost + HCost;
            Coords = coords;
        }
        public Node(int2 coords) : this(-1, int.MaxValue, default, coords) { }
        public Node(int nodeIndex, int gridWidth) : this(GetXY2(nodeIndex, gridWidth)) { }
    }
}
