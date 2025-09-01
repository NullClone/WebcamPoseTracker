using System;
using Unity.InferenceEngine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using WPT.Filters;
using WPT.Utilities;

namespace WPT
{
    public sealed class PoseDetection : MonoBehaviour
    {
        // Fields

        public Vector3[] BonePositions = new Vector3[NumKeypoints];
        public KalmanFilter[] KalmanFilters = new KalmanFilter[NumKeypoints];
        public LowPassFilter[] LowPassFilters = new LowPassFilter[NumKeypoints];

        public const int NumKeypoints = 33;
        public const int DetectorInputSize = 224;
        public const int LandmarkerInputSize = 256;


        [SerializeField] private ImageSource _imageSource;
        [SerializeField] private Keypoint[] _keypoints;
        [SerializeField] private PerformanceLevel _performanceLevel = PerformanceLevel.Full;
        [SerializeField] private BackendType _backendType = BackendType.GPUCompute;
        [SerializeField] private FilterMode _filterMode = FilterMode.None;
        [SerializeField] private float _scoreThreshold = 0.75f;
        [SerializeField] private float _timeInterval = 0.45f;
        [SerializeField] private float _noise = 0.4f;
        [SerializeField] private int _nOrder = 7;
        [SerializeField] private float _smooth = 0.9f;

        private Worker _detectorWorker;
        private Worker _landmarkerWorker;
        private Tensor<float> _detectorInput;
        private Tensor<float> _landmarkerInput;
        private Awaitable _executeAwaitable;
        private float2x3 M;
        private float2x3 M2;
        private float[,] _anchors;


        // Methods

        private void Awake()
        {
            for (int i = 0; i < NumKeypoints; i++)
            {
                KalmanFilters[i] = new KalmanFilter();
                KalmanFilters[i].SetParameter(_timeInterval, _noise);
                KalmanFilters[i].Predict();

                LowPassFilters[i] = new LowPassFilter(_nOrder, _smooth);
            }
        }

        private async void Start()
        {
            if (_imageSource == null) return;

            var detectorHandle = Addressables.LoadAssetAsync<ModelAsset>("Pose/Detection");

            await detectorHandle.Task;

            if (detectorHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var detectorModel = ModelLoader.Load(detectorHandle.Result);
                var graph = new FunctionalGraph();
                var input = graph.AddInput(detectorModel, 0);
                var outputs = Functional.Forward(detectorModel, input);
                var detectionScores = Functional.Sigmoid(Functional.Clamp(outputs[1], -100f, 100f));
                var bestScoreIndex = Functional.ArgMax(outputs[1], 1).Squeeze();
                var selectedBoxes = Functional.IndexSelect(outputs[0], 1, bestScoreIndex).Unsqueeze(0);
                var selectedScores = Functional.IndexSelect(detectionScores, 1, bestScoreIndex).Unsqueeze(0);

                _detectorWorker = new Worker(graph.Compile(bestScoreIndex, selectedScores, selectedBoxes), _backendType);
            }


            var landmarkerHandle = _performanceLevel switch
            {
                PerformanceLevel.Lite => Addressables.LoadAssetAsync<ModelAsset>("Pose/Landmarks_detector_lite"),
                PerformanceLevel.Full => Addressables.LoadAssetAsync<ModelAsset>("Pose/Landmarks_detector_full"),
                PerformanceLevel.Heavy => Addressables.LoadAssetAsync<ModelAsset>("Pose/Landmarks_detector_heavy"),

                _ => throw new ArgumentOutOfRangeException(nameof(_performanceLevel), _performanceLevel, null)
            };

            await landmarkerHandle.Task;

            if (landmarkerHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var landmarkerModel = ModelLoader.Load(landmarkerHandle.Result);

                _landmarkerWorker = new Worker(landmarkerModel, _backendType);
            }


            var anchorsHandle = Addressables.LoadAssetAsync<TextAsset>("Pose/Anchors");

            await anchorsHandle.Task;

            if (anchorsHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var anchors = anchorsHandle.Result.text.Split('\n');

                _anchors = new float[anchors.Length - 1, 4];

                for (int i = 0; i < anchors.Length - 1; i++)
                {
                    var anchorValues = anchors[i].Split(',');

                    for (int j = 0; j < 4; j++)
                    {
                        _anchors[i, j] = float.Parse(anchorValues[j]);
                    }
                }
            }

            if (_detectorWorker != null && _landmarkerWorker != null && _anchors != null)
            {
                _detectorInput = new Tensor<float>(new TensorShape(1, DetectorInputSize, DetectorInputSize, 3));
                _landmarkerInput = new Tensor<float>(new TensorShape(1, LandmarkerInputSize, LandmarkerInputSize, 3));

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

                _detectorInput.Dispose();
                _landmarkerInput.Dispose();
            }

            detectorHandle.Release();
            landmarkerHandle.Release();
            anchorsHandle.Release();
        }

        private async Awaitable ExecuteModel()
        {
            SetDetectorInput();

            _detectorWorker.Schedule(_detectorInput);

            using var outputIdx = await (_detectorWorker.PeekOutput(0) as Tensor<int>).ReadbackAndCloneAsync();
            using var outputScore = await (_detectorWorker.PeekOutput(1) as Tensor<float>).ReadbackAndCloneAsync();
            using var outputBox = await (_detectorWorker.PeekOutput(2) as Tensor<float>).ReadbackAndCloneAsync();

            if (outputScore[0] < _scoreThreshold) return;

            SetLandmarkerInput(outputIdx[0], outputBox);

            _landmarkerWorker.Schedule(_landmarkerInput);

            using var landmarks = await (_landmarkerWorker.PeekOutput(0) as Tensor<float>).ReadbackAndCloneAsync();

            SetKeypoints(landmarks);
        }

        private void SetDetectorInput()
        {
            var size = Mathf.Max(_imageSource.Resolution.x, _imageSource.Resolution.y);

            var scale = size / DetectorInputSize;

            var delta = 0.5f * (_imageSource.Resolution + new Vector2(-size, size));

            M = MatrixUtils.Multiply(
                MatrixUtils.TranslationMatrix(delta),
                MatrixUtils.ScaleMatrix(scale, -scale));

            ImageUtils.SampleImageAffine(_imageSource.Texture, _detectorInput, M);
        }

        private void SetLandmarkerInput(int idx, Tensor<float> box)
        {
            var anchorPosition = DetectorInputSize * new float2(_anchors[idx, 0], _anchors[idx, 1]);

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

            ImageUtils.SampleImageAffine(_imageSource.Texture, _landmarkerInput, M2);
        }

        private void SetKeypoints(Tensor<float> landmarks)
        {
            for (int i = 0; i < NumKeypoints; i++)
            {
                var imageSpacePosition = MatrixUtils.Multiply(M2, new float2(landmarks[(5 * i) + 0], landmarks[(5 * i) + 1]));

                var active = landmarks[(5 * i) + 3] > 0.5f && landmarks[(5 * i) + 4] > 0.5f;

                if (active)
                {
                    BonePositions[i] = new Vector3(
                        imageSpacePosition.x - (0.5f * _imageSource.Resolution.x),
                        imageSpacePosition.y - (0.5f * _imageSource.Resolution.y),
                        landmarks[(5 * i) + 2]) / _imageSource.Resolution.y;

                    if ((_filterMode & FilterMode.KalmanFilter) != 0)
                    {
                        BonePositions[i] = KalmanFilters[i].CorrectAndPredict(BonePositions[i]);
                    }

                    if ((_filterMode & FilterMode.LowPassFilter) != 0)
                    {
                        BonePositions[i] = LowPassFilters[i].CorrectAndPredict(BonePositions[i]);
                    }
                }

                if (_keypoints != null && _keypoints.Length == NumKeypoints)
                {
                    _keypoints[i].SetValue(BonePositions[i], active);
                }
            }
        }

        private void OnDestroy()
        {
            _executeAwaitable?.Cancel();
            _detectorWorker?.Dispose();
            _landmarkerWorker?.Dispose();
        }
    }

    [Flags]
    public enum FilterMode
    {
        None = 0,
        KalmanFilter = 1 << 0,
        LowPassFilter = 1 << 1,
    }

    enum PerformanceLevel
    {
        Lite,
        Full,
        Heavy,
    }
}