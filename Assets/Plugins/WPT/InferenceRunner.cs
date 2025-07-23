using System;
using Unity.InferenceEngine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using WPT.Utilities;

namespace WPT
{
    public sealed class InferenceRunner : MonoBehaviour
    {
        // Properties

        public Vector3[] BonePositions { get; private set; } = new Vector3[NumKeypoints];

        public float[,] Anchors { get; private set; }


        // Fields

        [SerializeField] private BackendType _backendType = BackendType.GPUCompute;
        [SerializeField] private TextAsset _anchors;
        [SerializeField] private ImageSource _imageSource;
        [SerializeField] private float _scoreThreshold = 0.75f;
        [SerializeField] private FilterMode _filterMode = FilterMode.None;
        [SerializeField] private float _kalmanParamQ;
        [SerializeField] private float _kalmanParamR;
        [SerializeField] private Keypoint[] _keypoints;

        private Worker _detectorWorker;
        private Worker _landmarkerWorker;
        private Tensor<float> _detectorInput;
        private Tensor<float> _landmarkerInput;
        private Awaitable _executeAwaitable;
        private float2x3 M;
        private float2x3 M2;

        private readonly float3x3[] _positions = new float3x3[NumKeypoints];

        private const int NumKeypoints = 33;
        private const int DetectorInputSize = 224;
        private const int LandmarkerInputSize = 256;


        // Methods

        private async void Start()
        {
            if (_imageSource == null || _anchors == null) return;

            var detectorHandle = Addressables.LoadAssetAsync<ModelAsset>("pose_detection");
            var landmarkerHandle = Addressables.LoadAssetAsync<ModelAsset>("pose_landmarks_detector_full");

            await detectorHandle.Task;
            await landmarkerHandle.Task;

            if (detectorHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var detectorModel = ModelLoader.Load(detectorHandle.Result);
                var graph = new FunctionalGraph();
                var input = graph.AddInput(detectorModel, 0);
                var outputs = Functional.Forward(detectorModel, input);
                var results = ModelUtils.ArgMaxFiltering(outputs[0], outputs[1]);

                _detectorWorker = new Worker(graph.Compile(results.Item1, results.Item2, results.Item3), _backendType);
            }

            if (landmarkerHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var landmarkerModel = ModelLoader.Load(landmarkerHandle.Result);

                _landmarkerWorker = new Worker(landmarkerModel, _backendType);
            }

            if (_detectorWorker == null || _landmarkerWorker == null) return;

            _detectorInput = new Tensor<float>(new TensorShape(1, DetectorInputSize, DetectorInputSize, 3));
            _landmarkerInput = new Tensor<float>(new TensorShape(1, LandmarkerInputSize, LandmarkerInputSize, 3));

            Anchors = ModelUtils.LoadAnchors(_anchors.text);

            while (true)
            {
                try
                {
                    _executeAwaitable = ExecuteModel();

                    await _executeAwaitable;
                }
                catch (OperationCanceledException)
                {
                    _detectorInput.Dispose();
                    _landmarkerInput.Dispose();

                    Addressables.Release(detectorHandle);
                    Addressables.Release(landmarkerHandle);

                    break;
                }
            }
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
            var anchorPosition = DetectorInputSize * new float2(Anchors[idx, 0], Anchors[idx, 1]);

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
                var ImageSpacePosition = MatrixUtils.Multiply(M2, new float2(landmarks[(5 * i) + 0], landmarks[(5 * i) + 1]));

                //if (landmarks[(5 * i) + 3] < 0.5f || landmarks[(5 * i) + 4] < 0.5f) return;

                BonePositions[i] = new Vector3(
                    ImageSpacePosition.x - (0.5f * _imageSource.Resolution.x),
                    ImageSpacePosition.y - (0.5f * _imageSource.Resolution.y),
                    landmarks[(5 * i) + 2]) / _imageSource.Resolution.y;

                if ((_filterMode & FilterMode.KalmanFilter) != 0)
                {
                    // KalmanK = c0 / KalmanP = c1 / KalmanX = c2

                    _positions[i].c0.x = (_positions[i].c1.x + _kalmanParamQ) / (_positions[i].c1.x + _kalmanParamQ + _kalmanParamR);
                    _positions[i].c0.y = (_positions[i].c1.y + _kalmanParamQ) / (_positions[i].c1.y + _kalmanParamQ + _kalmanParamR);
                    _positions[i].c0.z = (_positions[i].c1.z + _kalmanParamQ) / (_positions[i].c1.z + _kalmanParamQ + _kalmanParamR);

                    _positions[i].c1.x = _kalmanParamR * (_positions[i].c1.x + _kalmanParamQ) / (_kalmanParamR + _positions[i].c1.x + _kalmanParamQ);
                    _positions[i].c1.y = _kalmanParamR * (_positions[i].c1.y + _kalmanParamQ) / (_kalmanParamR + _positions[i].c1.y + _kalmanParamQ);
                    _positions[i].c1.z = _kalmanParamR * (_positions[i].c1.z + _kalmanParamQ) / (_kalmanParamR + _positions[i].c1.z + _kalmanParamQ);

                    BonePositions[i].x = _positions[i].c2.x + ((BonePositions[i].x - _positions[i].c2.x) * _positions[i].c0.x);
                    BonePositions[i].y = _positions[i].c2.y + ((BonePositions[i].y - _positions[i].c2.y) * _positions[i].c0.y);
                    BonePositions[i].z = _positions[i].c2.z + ((BonePositions[i].z - _positions[i].c2.z) * _positions[i].c0.z);

                    _positions[i].c2 = BonePositions[i];
                }

                if ((_filterMode & FilterMode.LowPassFilter) != 0)
                {
                    /*

                    _positions[0] = BonePositions[i];

                    for (int j = 1; j < _positions.Length; j++)
                    {
                        _positions[j] = (_positions[i] * LowPassParam) + (_positions[j - 1] * (1f - LowPassParam));
                    }

                    BonePositions[i] = _positions[^1];

                    */
                }
            }

            if (_keypoints != null && _keypoints.Length == NumKeypoints)
            {
                for (int i = 0; i < NumKeypoints; i++)
                {
                    _keypoints[i].SetValue(BonePositions[i], true);
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
}