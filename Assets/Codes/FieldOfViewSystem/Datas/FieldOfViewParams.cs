using System;

namespace Kaizerwald.FieldOfView
{
    [Serializable]
    public struct FieldOfViewParams
    {
        public float Range;
        public float SideAngleRadian;
        public float WidthLength;

        public FieldOfViewParams(float range, float sideAngleRadian, float widthLength)
        {
            Range = range;
            SideAngleRadian = sideAngleRadian;
            WidthLength = widthLength;
        }
    }
}