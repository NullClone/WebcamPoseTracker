using Unity.Mathematics;
using UnityEngine;

namespace WPT.Utilities
{
    public static class BlazeUtils
    {
        public static float[,] LoadAnchors(TextAsset textAsset)
        {
            var anchors = textAsset.text.Split('\n');

            var result = new float[anchors.Length - 1, 4];

            for (int i = 0; i < anchors.Length - 1; i++)
            {
                var anchorValues = anchors[i].Split(',');

                for (int j = 0; j < 4; j++)
                {
                    result[i, j] = float.Parse(anchorValues[j]);
                }
            }

            return result;
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