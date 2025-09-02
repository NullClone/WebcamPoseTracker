using System;
using Unity.InferenceEngine;
using Unity.Mathematics;
using UnityEngine;
using WPT.Utilities;

namespace WPT
{
    public sealed class HandDetection : MonoBehaviour
    {
        // Fields

        [SerializeField] private ImageSource _imageSource;
        [Space]
        [SerializeField] private BackendType _backendType = BackendType.GPUCompute;
        [SerializeField, Range(0f, 1f)] private float _scoreThreshold = 0.5f;
        [Space]
        [SerializeField] private Keypoint[] _keypoints;

        private Worker _detectorWorker;
        private Worker _landmarkerWorker;
        private Tensor<float> _detectorInput;
        private Tensor<float> _landmarkerInput;
        private Awaitable _detectAwaitable;
        private float2x3 M;
        private float2x3 M2;
        private float[,] _anchors;

        private const int NumKeypoints = 21;
        private const int DetectorInputSize = 192;
        private const int LandmarkerInputSize = 224;


        // Properties

        public Vector3[] Positions { get; set; } = new Vector3[NumKeypoints];


        // Methods

        private async void Start()
        {
            if (_imageSource == null) return;

            var detectorRequest = Resources.LoadAsync<ModelAsset>("ONNX/Hand/hand_detector.onnx");

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


            var landmarkerRequest = Resources.LoadAsync<ModelAsset>("ONNX/Hand/hand_landmarks_detector.onnx");

            await landmarkerRequest;

            var landmarkerModel = ModelLoader.Load((ModelAsset)landmarkerRequest.asset);

            _landmarkerWorker = new Worker(landmarkerModel, _backendType);


            var anchorsRequest = Resources.LoadAsync<TextAsset>("Anchors/HandAnchors.csv");

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

            var boxCentre_TensorSpace = anchorPosition + new float2(box[0, 0, 0], box[0, 0, 1]);
            var boxSize_TensorSpace = math.max(box[0, 0, 2], box[0, 0, 3]);

            var kp0_TensorSpace = anchorPosition + new float2(box[0, 0, 4 + (2 * 0) + 0], box[0, 0, 4 + (2 * 0) + 1]);
            var kp2_TensorSpace = anchorPosition + new float2(box[0, 0, 4 + (2 * 2) + 0], box[0, 0, 4 + (2 * 2) + 1]);
            var delta_TensorSpace = kp2_TensorSpace - kp0_TensorSpace;
            var up_TensorSpace = delta_TensorSpace / math.length(delta_TensorSpace);
            var theta = math.atan2(delta_TensorSpace.y, delta_TensorSpace.x);
            var rotation = (0.5f * Mathf.PI) - theta;
            boxCentre_TensorSpace += 0.5f * boxSize_TensorSpace * up_TensorSpace;
            boxSize_TensorSpace *= 2.6f;

            var origin2 = new float2(0.5f * LandmarkerInputSize, 0.5f * LandmarkerInputSize);
            var scale2 = boxSize_TensorSpace / LandmarkerInputSize;

            M2 = BlazeUtils.Multiply(M,
                    BlazeUtils.Multiply(
                        BlazeUtils.Multiply(
                            BlazeUtils.Multiply(
                                BlazeUtils.TranslationMatrix(boxCentre_TensorSpace),
                                BlazeUtils.ScaleMatrix(scale2, -scale2)),
                            BlazeUtils.RotationMatrix(rotation)),
                        BlazeUtils.TranslationMatrix(-origin2)));

            ImageUtils.SampleImageAffine(_imageSource.Texture, _landmarkerInput, M2);
        }

        private void SetKeypoints(Tensor<float> landmarks)
        {
            for (int i = 0; i < NumKeypoints; i++)
            {
                var imageSpacePosition = BlazeUtils.Multiply(M2, new float2(landmarks[(3 * i) + 0], landmarks[(3 * i) + 1]));

                Positions[i] = new Vector3(
                        imageSpacePosition.x - (0.5f * _imageSource.Resolution.x),
                        imageSpacePosition.y - (0.5f * _imageSource.Resolution.y),
                        landmarks[(3 * i) + 2]) / _imageSource.Resolution.y;

                if (_keypoints != null && _keypoints.Length == NumKeypoints)
                {
                    _keypoints[i].SetValue(Positions[i], true);
                }
            }
        }

        private void OnDestroy()
        {
            _detectAwaitable?.Cancel();
            _detectorWorker?.Dispose();
            _landmarkerWorker?.Dispose();
        }
    }
}