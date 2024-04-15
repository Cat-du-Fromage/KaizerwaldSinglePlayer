using Unity.Mathematics;

namespace Kaizerwald
{
    public readonly struct FieldOfViewData
    {
        public readonly float2 LeftStartPosition; //unit most left
        public readonly float2 RightStartPosition; //unit most Right
        
        public readonly float2 LeftDirection;
        public readonly float2 RightDirection;

        public readonly float2 TriangleTip;
        public readonly float Radius;
        
        public FieldOfViewData(float2 leftStartPosition, float2 rightStartPosition, float2 leftConeDirection, float2 rightConeDirection, float2 triangleTip, float radius)
        {
            LeftStartPosition  = leftStartPosition;
            RightStartPosition = rightStartPosition;
            LeftDirection      = leftConeDirection;
            RightDirection     = rightConeDirection;
            TriangleTip        = triangleTip;
            Radius             = radius;
        }
    }
}