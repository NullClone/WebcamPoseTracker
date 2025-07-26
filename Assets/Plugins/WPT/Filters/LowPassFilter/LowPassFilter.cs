using UnityEngine;

namespace WPT.Filters
{
    public class LowPassFilter
    {
        // Fields

        private readonly Vector3[] prevPos3D = new Vector3[10];
        private readonly int NOrderLPF = 7;
        private readonly float Smooth = 0.9f;

        private int effectiveCount;


        // Methods

        public LowPassFilter(int order, float smooth)
        {
            NOrderLPF = Mathf.Min(order, 10);
            Smooth = smooth;
        }

        public Vector3 CorrectAndPredict(Vector3 value)
        {
            prevPos3D[0] = value;

            for (int i = 1; i < NOrderLPF; i++)
            {
                prevPos3D[i] = (prevPos3D[i] * (1f - Smooth)) + (prevPos3D[i - 1] * Smooth);
            }

            prevPos3D[0] = (prevPos3D[0] * (1f - Smooth)) + (prevPos3D[NOrderLPF - 1] * Smooth);

            if (effectiveCount < 10)
            {
                effectiveCount++;

                return value;
            }

            return prevPos3D[0];
        }
    }
}