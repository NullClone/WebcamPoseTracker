using Unity.Mathematics;
using UnityEngine;

namespace WPT.Utilities
{
    public static class MatrixUtils
    {
        public static Quaternion LookRotation(Vector3 forward)
        {
            if (forward == Vector3.zero)
            {
                return Quaternion.identity;
            }

            return Quaternion.LookRotation(forward);
        }

        public static Quaternion LookRotation(Vector3 forward, Vector3 upwards)
        {
            if (forward == Vector3.zero)
            {
                return Quaternion.identity;
            }

            return Quaternion.LookRotation(forward, upwards);
        }

        public static Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
        {
            var result = Vector3.Cross(a - b, a - c);

            return result.normalized;
        }


        public static float2x3 Multiply(float2x3 a, float2x3 b)
        {
            return new float2x3(
                (a[0][0] * b[0][0]) + (a[1][0] * b[0][1]),
                (a[0][0] * b[1][0]) + (a[1][0] * b[1][1]),
                (a[0][0] * b[2][0]) + (a[1][0] * b[2][1]) + a[2][0],
                (a[0][1] * b[0][0]) + (a[1][1] * b[0][1]),
                (a[0][1] * b[1][0]) + (a[1][1] * b[1][1]),
                (a[0][1] * b[2][0]) + (a[1][1] * b[2][1]) + a[2][1]
            );
        }

        public static float2 Multiply(float2x3 a, float2 b)
        {
            return new float2(
                (a[0][0] * b.x) + (a[1][0] * b.y) + a[2][0],
                (a[0][1] * b.x) + (a[1][1] * b.y) + a[2][1]
            );
        }

        public static float2x3 RotationMatrix(float theta)
        {
            var sinTheta = math.sin(theta);
            var cosTheta = math.cos(theta);

            return new float2x3(cosTheta, -sinTheta, 0, sinTheta, cosTheta, 0);
        }

        public static float2x3 TranslationMatrix(float2 delta)
        {
            return new float2x3(1, 0, delta.x, 0, 1, delta.y);
        }

        public static float2x3 ScaleMatrix(float x, float y)
        {
            return new float2x3(x, 0, 0, 0, y, 0);
        }
    }
}