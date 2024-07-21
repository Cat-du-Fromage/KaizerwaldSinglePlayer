using Unity.Mathematics;
using static Unity.Mathematics.math;
using static Kaizerwald.FieldOfView.FovUtils;

namespace Kaizerwald.FieldOfView
{
    public readonly struct FieldOfViewBounds
    {
        public readonly FieldOfViewParams Params;
        public readonly float2 FrontForward;
        public readonly float2x4 TrapezePoints;

        public FieldOfViewBounds(FieldOfViewParams fovParams, float2 position, float2 forward)
        {
            Params = fovParams;
            FrontForward = forward * Params.Range;
            float2 origin = new float2(0, Params.Range);
            
            // ------------------------ Small Bases ------------------------ 
            float2 smallBaseLeft  = new float2(-Params.WidthLength / 2, 0);
            float2 smallBaseRight = new float2(Params.WidthLength / 2, 0);
            
            // ------------------------ Big Bases ------------------------ 
            float2 rightDirection = new float2(cos(Params.SideAngleRadian), sin(Params.SideAngleRadian));
            float2 leftDirection  = new float2(cos(PI - Params.SideAngleRadian), sin(PI - Params.SideAngleRadian));
            // right = (1,0) => determinant(float2x2(right().xz, rightDirection)) = 1 * rightDirection.y - 0
            float rightScalar = determinant(float2x2(smallBaseRight - origin, rightDirection)) / rightDirection.y;
            // left = (-1,0) => determinant(float2x2(left().xz, leftDirection)) = -1 * leftDirection.y - 0
            float leftScalar = determinant(float2x2(smallBaseLeft - origin, leftDirection)) / -leftDirection.y;
            float2 bigBaseRight = new float2(rightScalar, Params.Range);
            float2 bigBaseLeft  = new float2(-leftScalar, Params.Range);
            
            // ------------------------ Transformed points ------------------------ 
            sincos(acos(forward.y) * sign(-forward.x), out float sinA, out float cosA);
            float2x2 rotationMatrix = new float2x2(cosA, -sinA, sinA,  cosA);
            TrapezePoints = new float2x4
            {
                c0 = mul(rotationMatrix, smallBaseLeft)  + position,
                c1 = mul(rotationMatrix, smallBaseRight) + position,
                c2 = mul(rotationMatrix, bigBaseRight)   + position,
                c3 = mul(rotationMatrix, bigBaseLeft)    + position
            };
        }
        public FieldOfViewBounds(FieldOfViewParams fovParams, float3 position, float3 forward) : this(fovParams, position.xz, forward.xz) { }

        public bool IsPointInsideFov(float2 point)
        {
            float2x4 edgesToPoint = float2x4(point, point, point, point) - TrapezePoints;
            bool4 trapezeEdgeChecks = new bool4
            (
                determinant(float2x2(TrapezePoints[1] - TrapezePoints[0], edgesToPoint[0])) > 0,
                determinant(float2x2(TrapezePoints[2] - TrapezePoints[1], edgesToPoint[1])) > 0,
                determinant(float2x2(TrapezePoints[3] - TrapezePoints[2], edgesToPoint[2])) > 0,
                determinant(float2x2(TrapezePoints[0] - TrapezePoints[3], edgesToPoint[3])) > 0
            );
            if (!all(trapezeEdgeChecks)) return false;

            bool isLeftArc = determinant(float2x2(FrontForward, edgesToPoint[0])) > 0;
            if (isLeftArc) return distance(TrapezePoints[0], point) <= Params.Range;
            
            bool isRightArc = determinant(float2x2(FrontForward, edgesToPoint[1])) < 0;
            if (isRightArc) return distance(TrapezePoints[1], point) <= Params.Range;
            
            return true;
        }
        
        public bool IsPointInsideFov(float3 point)
        {
            return IsPointInsideFov(point.xz);
        }
    }
}