using System;
using Unity.InferenceEngine;
using Unity.Mathematics;
using UnityEngine;
using WPT.Utilities;

namespace WPT
{
    public sealed class InferenceRunner : MonoBehaviour
    {
        // Properties

        public Vector3[] BonePositions { get; private set; } = new Vector3[NumKeypoints];


        // Fields

        [SerializeField] private ModelResource _model;
        [SerializeField] private ImageSource _imageSource;
        [SerializeField] private float _scoreThreshold = 0.75f;
        [SerializeField] private float _kalmanParamQ;
        [SerializeField] private float _kalmanParamR;
        [SerializeField] private Keypoint[] _keypoints;
        public FilterMode _filterMode = FilterMode.KalmanFilter;

        private Tensor<float> _detectorInput;
        private Tensor<float> _landmarkerInput;
        private Awaitable _executeAwaitable;
        private float2x3 M;
        private float2x3 M2;
        private Vector3[] _currentPositions = new Vector3[NumKeypoints];
        private float3x3[] _positions = new float3x3[NumKeypoints];
        private bool _isInitialized;


        public const int NumKeypoints = 33;
        public const int DetectorInputSize = 224;
        public const int LandmarkerInputSize = 256;


        // Async Methods

        private async void Start()
        {
            if (!_isInitialized) return;

            while (true)
            {
                try
                {
                    _executeAwaitable = ExecuteModel();

                    await _executeAwaitable;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Awaitable ExecuteModel()
        {
            if (_model.DetectorWorker == null || _model.LandmarkerWorker == null) return;

            ImageUtils.SampleImageAffine(_imageSource.Texture, _detectorInput, M);

            _model.DetectorWorker.Schedule(_detectorInput);

            using var outputIdx = await (_model.DetectorWorker.PeekOutput(0) as Tensor<int>).ReadbackAndCloneAsync();
            using var outputScore = await (_model.DetectorWorker.PeekOutput(1) as Tensor<float>).ReadbackAndCloneAsync();
            using var outputBox = await (_model.DetectorWorker.PeekOutput(2) as Tensor<float>).ReadbackAndCloneAsync();

            if (outputScore[0] >= _scoreThreshold)
            {
                SetLandmarkerInput(outputIdx[0], outputBox);
            }
            else return;

            ImageUtils.SampleImageAffine(_imageSource.Texture, _landmarkerInput, M2);

            _model.LandmarkerWorker.Schedule(_landmarkerInput);

            using var landmarks = await (_model.LandmarkerWorker.PeekOutput(0) as Tensor<float>).ReadbackAndCloneAsync();

            SetKeypoints(landmarks);

            if (_keypoints != null && _keypoints.Length == NumKeypoints)
            {
                for (int i = 0; i < NumKeypoints; i++)
                {
                    _keypoints[i].SetValue(BonePositions[i], true);
                }
            }
        }


        // Methods

        private void Awake()
        {
            if (_model == null || _imageSource == null) return;

            _model.Initialize();

            SetDetectorInput();

            _detectorInput = new Tensor<float>(new TensorShape(1, DetectorInputSize, DetectorInputSize, 3));
            _landmarkerInput = new Tensor<float>(new TensorShape(1, LandmarkerInputSize, LandmarkerInputSize, 3));

            _isInitialized = true;
        }

        private void OnDestroy()
        {
            _executeAwaitable?.Cancel();

            if (_detectorInput != null)
            {
                _detectorInput.Dispose();
                _detectorInput = null;
            }

            if (_landmarkerInput != null)
            {
                _landmarkerInput.Dispose();
                _landmarkerInput = null;
            }

            if (_model != null)
            {
                _model.Dispose();
            }
        }

        private void SetDetectorInput()
        {
            var size = Mathf.Max(_imageSource.Resolution.x, _imageSource.Resolution.y);

            var scale = size / DetectorInputSize;

            var delta = 0.5f * (_imageSource.Resolution + new Vector2(-size, size));

            M = MatrixUtils.Multiply(
                MatrixUtils.TranslationMatrix(delta),
                MatrixUtils.ScaleMatrix(scale, -scale));
        }

        private void SetLandmarkerInput(int idx, Tensor<float> box)
        {
            var anchorPosition = DetectorInputSize * new float2(_model.Anchors[idx, 0], _model.Anchors[idx, 1]);

            var kp1 = MatrixUtils.Multiply(M, anchorPosition + new float2(box[0, 0, 4], box[0, 0, 5]));
            var kp2 = MatrixUtils.Multiply(M, anchorPosition + new float2(box[0, 0, 6], box[0, 0, 7]));
            var delta = kp2 - kp1;

            var halfInputSize = 0.5f * LandmarkerInputSize;
            var scale = 1.25f * math.length(delta) / halfInputSize;
            var theta = (0.5f * Mathf.PI) - math.atan2(delta.y, delta.x);

            M2 = MatrixUtils.Multiply(
                     MatrixUtils.Multiply(
                         MatrixUtils.Multiply(
                             MatrixUtils.TranslationMatrix(kp1),
                             MatrixUtils.ScaleMatrix(scale, -scale)),
                         MatrixUtils.RotationMatrix(theta)),
                     MatrixUtils.TranslationMatrix(-new float2(halfInputSize, halfInputSize)));
        }

        private void SetKeypoints(Tensor<float> landmarks)
        {
            for (int i = 0; i < NumKeypoints; i++)
            {
                var ImageSpacePosition = MatrixUtils.Multiply(M2, new float2(landmarks[(5 * i) + 0], landmarks[(5 * i) + 1]));

                //if (landmarks[(5 * i) + 3] < 0.5f || landmarks[(5 * i) + 4] < 0.5f) return;

                _currentPositions[i] = new Vector3(
                    ImageSpacePosition.x - (0.5f * _imageSource.Resolution.x),
                    ImageSpacePosition.y - (0.5f * _imageSource.Resolution.y),
                    landmarks[(5 * i) + 2]) / _imageSource.Resolution.y;

                // KalmanFilter (KalmanK = c0 / KalmanP = c1 / KalmanX = c2)

                _positions[i].c0.x = (_positions[i].c1.x + _kalmanParamQ) / (_positions[i].c1.x + _kalmanParamQ + _kalmanParamR);
                _positions[i].c0.y = (_positions[i].c1.y + _kalmanParamQ) / (_positions[i].c1.y + _kalmanParamQ + _kalmanParamR);
                _positions[i].c0.z = (_positions[i].c1.z + _kalmanParamQ) / (_positions[i].c1.z + _kalmanParamQ + _kalmanParamR);

                _positions[i].c1.x = _kalmanParamR * (_positions[i].c1.x + _kalmanParamQ) / (_kalmanParamR + _positions[i].c1.x + _kalmanParamQ);
                _positions[i].c1.y = _kalmanParamR * (_positions[i].c1.y + _kalmanParamQ) / (_kalmanParamR + _positions[i].c1.y + _kalmanParamQ);
                _positions[i].c1.z = _kalmanParamR * (_positions[i].c1.z + _kalmanParamQ) / (_kalmanParamR + _positions[i].c1.z + _kalmanParamQ);

                BonePositions[i].x = _positions[i].c2.x + ((_currentPositions[i].x - _positions[i].c2.x) * _positions[i].c0.x);
                BonePositions[i].y = _positions[i].c2.y + ((_currentPositions[i].y - _positions[i].c2.y) * _positions[i].c0.y);
                BonePositions[i].z = _positions[i].c2.z + ((_currentPositions[i].z - _positions[i].c2.z) * _positions[i].c0.z);
                _positions[i].c2 = BonePositions[i];
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