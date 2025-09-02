using System;
using Unity.InferenceEngine;
using Unity.Mathematics;
using UnityEngine;
using WPT.Utilities;

namespace WPT
{
    public sealed class PoseDetection : MonoBehaviour
    {
        // Fields

        [SerializeField] private ImageSource _imageSource;
        [Space]
        [SerializeField] private PerformanceLevel _performanceLevel = PerformanceLevel.Full;
        [SerializeField] private BackendType _backendType = BackendType.GPUCompute;
        [SerializeField, Range(0f, 1f)] private float _scoreThreshold = 0.75f;
        [Space]
        [SerializeField] private Keypoint[] _keypoints;

        private DetectionManager _manager;
        private Worker _detectorWorker;
        private Worker _landmarkerWorker;
        private Tensor<float> _detectorInput;
        private Tensor<float> _landmarkerInput;
        private Awaitable _detectAwaitable;
        private float2x3 M;
        private float2x3 M2;
        private float[,] _anchors;

        private const int NumKeypoints = 33;
        private const int DetectorInputSize = 224;
        private const int LandmarkerInputSize = 256;


        // Properties

        public Vector3[] Positions { get; set; } = new Vector3[NumKeypoints];


        // Methods

        private async void Start()
        {
            if (_imageSource == null) return;

            _manager = DetectionManager.Instance;

            var detectorRequest = Resources.LoadAsync<ModelAsset>("ONNX/Pose/pose_detection.onnx");

            await detectorRequest;

            var detectorModel = ModelLoader.Load((ModelAsset)detectorRequest.asset);
            var graph = new FunctionalGraph();
            var input = graph.AddInput(detectorModel, 0);
            var outputs = Functional.Forward(detectorModel, input);
            var detectionScores = Functional.Sigmoid(Functional.Clamp(outputs[1], -100f, 100f));
            var bestScoreIndex = Functional.ArgMax(outputs[1], 1).Squeeze();
            var selectedBoxes = Functional.IndexSelect(outputs[0], 1, bestScoreIndex).Unsqueeze(0);
            var selectedScores = Functional.IndexSelect(detectionScores, 1, bestScoreIndex).Unsqueeze(0);

            _detectorWorker = new Worker(graph.Compile(bestScoreIndex, selectedScores, selectedBoxes), _backendType);


            var landmarkerRequest = _performanceLevel switch
            {
                PerformanceLevel.Lite => Resources.LoadAsync<ModelAsset>("ONNX/Pose/pose_landmarks_detector_lite.onnx"),
                PerformanceLevel.Full => Resources.LoadAsync<ModelAsset>("ONNX/Pose/pose_landmarks_detector_full.onnx"),
                PerformanceLevel.Heavy => Resources.LoadAsync<ModelAsset>("ONNX/Pose/pose_landmarks_detector_heavy.onnx"),

                _ => throw new NotImplementedException()
            };

            await landmarkerRequest;

            var landmarkerModel = ModelLoader.Load((ModelAsset)landmarkerRequest.asset);

            _landmarkerWorker = new Worker(landmarkerModel, _backendType);


            var anchorsRequest = Resources.LoadAsync<TextAsset>("Anchors/PoseAnchors.csv");

            await anchorsRequest;

            _anchors = BlazeUtils.LoadAnchors((TextAsset)anchorsRequest.asset);


            if (_detectorWorker != null && _landmarkerWorker != null && _anchors != null)
            {
                _detectorInput = new Tensor<float>(new TensorShape(1, DetectorInputSize, DetectorInputSize, 3));
                _landmarkerInput = new Tensor<float>(new TensorShape(1, LandmarkerInputSize, LandmarkerInputSize, 3));

                while (true)
                {
                    try
                    {
                        _detectAwaitable = Detect();

                        await _detectAwaitable;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                _detectorInput.Dispose();
                _landmarkerInput.Dispose();
            }

            Resources.UnloadAsset(detectorRequest.asset);
            Resources.UnloadAsset(landmarkerRequest.asset);
            Resources.UnloadAsset(anchorsRequest.asset);
        }

        private async Awaitable Detect()
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

            M = BlazeUtils.Multiply(
                    BlazeUtils.TranslationMatrix(delta),
                    BlazeUtils.ScaleMatrix(scale, -scale));

            ImageUtils.SampleImageAffine(_imageSource.Texture, _detectorInput, M);
        }

        private void SetLandmarkerInput(int idx, Tensor<float> box)
        {
            var anchorPosition = DetectorInputSize * new float2(_anchors[idx, 0], _anchors[idx, 1]);

            var kp1 = BlazeUtils.Multiply(M, anchorPosition + new float2(box[0, 0, 4], box[0, 0, 5]));
            var kp2 = BlazeUtils.Multiply(M, anchorPosition + new float2(box[0, 0, 6], box[0, 0, 7]));
            var delta = kp2 - kp1;

            var halfInputSize = 0.5f * LandmarkerInputSize;
            var scale = 1.25f * math.length(delta) / halfInputSize;
            var theta = (0.5f * Mathf.PI) - math.atan2(delta.y, delta.x);

            M2 = BlazeUtils.Multiply(
                       BlazeUtils.Multiply(
                           BlazeUtils.Multiply(
                               BlazeUtils.TranslationMatrix(kp1),
                               BlazeUtils.ScaleMatrix(scale, -scale)),
                           BlazeUtils.RotationMatrix(theta)),
                       BlazeUtils.TranslationMatrix(-new float2(halfInputSize, halfInputSize)));

            ImageUtils.SampleImageAffine(_imageSource.Texture, _landmarkerInput, M2);
        }

        private void SetKeypoints(Tensor<float> landmarks)
        {
            for (int i = 0; i < NumKeypoints; i++)
            {
                var imageSpacePosition = BlazeUtils.Multiply(M2, new float2(landmarks[(5 * i) + 0], landmarks[(5 * i) + 1]));

                var active = landmarks[(5 * i) + 3] > 0.5f && landmarks[(5 * i) + 4] > 0.5f;

                if (active)
                {
                    Positions[i] = new Vector3(
                        imageSpacePosition.x - (0.5f * _imageSource.Resolution.x),
                        imageSpacePosition.y - (0.5f * _imageSource.Resolution.y),
                        landmarks[(5 * i) + 2]) / _imageSource.Resolution.y;
                }

                if (_keypoints != null && _keypoints.Length == NumKeypoints)
                {
                    _keypoints[i].SetValue(Positions[i], active);
                }
            }

            _manager.SetFilter(Positions);
        }

        private void OnDestroy()
        {
            _detectAwaitable?.Cancel();
            _detectorWorker?.Dispose();
            _landmarkerWorker?.Dispose();
        }
    }

    public enum PerformanceLevel
    {
        Lite,
        Full,
        Heavy,
    }
}