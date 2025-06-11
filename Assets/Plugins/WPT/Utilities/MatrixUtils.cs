using Unity.Mathematics;

namespace WPT.Utilities
{
    public static class MatrixUtils
    {
        public static float2x3 Mul(float2x3 a, float2x3 b)
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

        public static float2 Mul(float2x3 a, float2 b)
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

        public static float2x3 ScaleMatrix(float2 scale)
        {
            return new float2x3(scale.x, 0, 0, 0, scale.y, 0);
        }
    }
}