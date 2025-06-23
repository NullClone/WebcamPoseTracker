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

        public Vector3[] Keypoints { get; set; } = new Vector3[NumKeypoints];

        public const int NumKeypoints = 33;
        public const int DetectorInputSize = 224;
        public const int LandmarkerInputSize = 256;


        // Fields

        [SerializeField] private ModelResource _model;
        [SerializeField] private ImageSource _imageSource;
        [SerializeField] private float _scoreThreshold = 0.75f;

        private Tensor<float> _detectorInput;
        private Tensor<float> _landmarkerInput;
        private Awaitable _executeAwaitable;
        private float2x3 M;
        private float2x3 M2;
        private bool _isInitialized;


        // Async Methods

        private async void Start()
        {
            if (_isInitialized)
            {
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
        }

        private async Awaitable ExecuteModel()
        {
            if (_model.DetectorWorker == null || _model.LandmarkerWorker == null) return;

            SetDetectorInput();

            _model.DetectorWorker.Schedule(_detectorInput);

            using var outputIdx = await (_model.DetectorWorker.PeekOutput(0) as Tensor<int>).ReadbackAndCloneAsync();
            using var outputScore = await (_model.DetectorWorker.PeekOutput(1) as Tensor<float>).ReadbackAndCloneAsync();
            using var outputBox = await (_model.DetectorWorker.PeekOutput(2) as Tensor<float>).ReadbackAndCloneAsync();

            if (outputScore[0] < _scoreThreshold) return;

            SetLandmarkerInput(outputIdx, outputBox);

            _model.LandmarkerWorker.Schedule(_landmarkerInput);

            using var landmarks = await (_model.LandmarkerWorker.PeekOutput(0) as Tensor<float>).ReadbackAndCloneAsync();

            SetKeypoint(landmarks);
        }


        // Methods

        private void Awake()
        {
            if (_model == null || _imageSource == null) return;

            _model.Initialize();

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

            M = MatrixUtils.Mul(
                MatrixUtils.TranslationMatrix(delta),
                MatrixUtils.ScaleMatrix(scale, -scale));

            ImageUtils.SampleImageAffine(_imageSource.Texture, _detectorInput, M);
        }

        private void SetLandmarkerInput(Tensor<int> idx, Tensor<float> box)
        {
            var anchorPosition = DetectorInputSize * new float2(_model.Anchors[idx[0], 0], _model.Anchors[idx[0], 1]);

            var kp1 = MatrixUtils.Mul(M, anchorPosition + new float2(box[0, 0, 4], box[0, 0, 5]));
            var kp2 = MatrixUtils.Mul(M, anchorPosition + new float2(box[0, 0, 6], box[0, 0, 7]));
            var delta = kp2 - kp1;

            var theta = math.atan2(delta.y, delta.x);
            var origin = new float2(0.5f * LandmarkerInputSize, 0.5f * LandmarkerInputSize);
            var scale = 1.25f * math.length(delta) / (0.5f * LandmarkerInputSize);

            var f1 = MatrixUtils.Mul(
                        MatrixUtils.TranslationMatrix(kp1),
                        MatrixUtils.ScaleMatrix(scale, -scale));

            var f2 = MatrixUtils.Mul(f1, MatrixUtils.RotationMatrix((0.5f * Mathf.PI) - theta));

            M2 = MatrixUtils.Mul(f2, MatrixUtils.TranslationMatrix(-origin));

            ImageUtils.SampleImageAffine(_imageSource.Texture, _landmarkerInput, M2);
        }

        private void SetKeypoint(Tensor<float> landmarks)
        {
            for (int i = 0; i < NumKeypoints; i++)
            {
                Vector2 position_ImageSpace = MatrixUtils.Mul(M2, new float2(landmarks[(5 * i) + 0], landmarks[(5 * i) + 1]));
                Vector3 position_WorldSpace = new Vector3(
                    position_ImageSpace.x - (0.5f * _imageSource.Resolution.x),
                    position_ImageSpace.y - (0.5f * _imageSource.Resolution.y),
                    landmarks[(5 * i) + 2]) / _imageSource.Resolution.y;

                if (landmarks[(5 * i) + 3] > 0.5f && landmarks[(5 * i) + 4] > 0.5f)
                {
                    Keypoints[i] = position_WorldSpace;
                }
            }
        }
    }
}