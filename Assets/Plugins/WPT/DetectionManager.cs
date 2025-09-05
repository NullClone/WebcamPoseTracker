using System;
using UnityEngine;
using WPT.Filters;

namespace WPT
{
    public sealed class DetectionManager : SingletonMonoBehaviour<DetectionManager>
    {
        // Fields

        [SerializeField] private FilterMode _filterMode = FilterMode.None;
        [SerializeField] private float _timeInterval = 0.45f;
        [SerializeField] private float _noise = 0.4f;
        [SerializeField] private int _nOrder = 7;
        [SerializeField] private float _smooth = 0.9f;


        // Properties

        public Vector3[] Positions { get; set; }

        public bool[] Actives { get; set; }

        public KalmanFilter[] KalmanFilters { get; set; }

        public LowPassFilter[] LowPassFilters { get; set; }


        // Methods

        private void Awake()
        {
            KalmanFilters = new KalmanFilter[33];
            LowPassFilters = new LowPassFilter[33];
            Actives = new bool[33];

            for (int i = 0; i < 33; i++)
            {
                KalmanFilters[i] = new KalmanFilter();
                KalmanFilters[i].SetParameter(_timeInterval, _noise);
                KalmanFilters[i].Predict();

                LowPassFilters[i] = new LowPassFilter(_nOrder, _smooth);
            }
        }

        public override void Update()
        {
            base.Update();
        }

        public void SetFilter(Vector3[] value)
        {
            Positions = value;

            for (int i = 0; i < 33; i++)
            {
                if ((_filterMode & FilterMode.KalmanFilter) != 0)
                {
                    Positions[i] = KalmanFilters[i].CorrectAndPredict(Positions[i]);
                }

                if ((_filterMode & FilterMode.LowPassFilter) != 0)
                {
                    Positions[i] = LowPassFilters[i].CorrectAndPredict(Positions[i]);
                }
            }
        }
    }

    [Flags]
    public enum FilterMode
    {
        None = 0,
        KalmanFilter = 1 << 0,
        LowPassFilter = 1 << 1,
    }
}