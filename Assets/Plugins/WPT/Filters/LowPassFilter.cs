using UnityEngine;

namespace WPT.Filters
{
    public class LowPassFilter
    {
        // Fields

        private readonly int _nOrder = 7;
        private readonly float _smooth = 0.9f;
        private readonly Vector3[] _prevPositions = new Vector3[10];


        // Methods

        public LowPassFilter(int nOrder, float smooth)
        {
            _nOrder = Mathf.Min(nOrder, 10);
            _smooth = smooth;
        }

        public Vector3 CorrectAndPredict(Vector3 value)
        {
            _prevPositions[0] = value;

            for (int i = 1; i < _nOrder; i++)
            {
                _prevPositions[i] = (_prevPositions[i] * (1f - _smooth)) + (_prevPositions[i - 1] * _smooth);
            }

            _prevPositions[0] = (_prevPositions[0] * (1f - _smooth)) + (_prevPositions[_nOrder - 1] * _smooth);

            return _prevPositions[0];
        }
    }
}