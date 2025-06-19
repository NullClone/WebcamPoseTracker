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


        // Fields

        [SerializeField] private ModelAssetLoader _assetLoader;
        [SerializeField] private ImageSource _imageSource;
        [SerializeField] private Avatar _avatar;

        private Tensor<float> _detectorInput;
        private Tensor<float> _landmarkerInput;
        private Awaitable _executeAwaitable;
        private float2x3 M;
        private float2x3 M2;


        public const int NumKeypoints = 33;
        public const int DetectorInputSize = 224;
        public const int LandmarkerInputSize = 256;


        // Methods

        private async void Start()
        {
            if (_assetLoader == null || _imageSource == null) return;

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
        }

        private async Awaitable ExecuteModel()
        {
            var detectorWorker = _assetLoader.DetectorWorker;

            if (detectorWorker == null) return;

            SetDetectorInput();

            detectorWorker.Schedule(_detectorInput);

            using var outputIdx = await (detectorWorker.PeekOutput(0) as Tensor<int>).ReadbackAndCloneAsync();
            using var outputScore = await (detectorWorker.PeekOutput(1) as Tensor<float>).ReadbackAndCloneAsync();
            using var outputBox = await (detectorWorker.PeekOutput(2) as Tensor<float>).ReadbackAndCloneAsync();

            if (outputScore[0] < 0.75f) return;

            var landmarkerWorker = _assetLoader.LandmarkerWorker;

            if (landmarkerWorker == null) return;

            SetLandmarkerInput(outputIdx, outputBox);

            landmarkerWorker.Schedule(_landmarkerInput);

            using var landmarks = await (landmarkerWorker.PeekOutput(0) as Tensor<float>).ReadbackAndCloneAsync();

            SetKeypoint(landmarks);
        }

        private void SetDetectorInput()
        {
            var size = Mathf.Max(_imageSource.Resolution.x, _imageSource.Resolution.y);

            var scale = size / (float)DetectorInputSize;

            var delta = 0.5f * (_imageSource.Resolution + new Vector2(-size, size));

            M = MatrixUtils.Mul(
                MatrixUtils.TranslationMatrix(delta),
                MatrixUtils.ScaleMatrix(scale, -scale));

            ImageUtils.SampleImageAffine(_imageSource.Texture, _detectorInput, M);
        }

        private void SetLandmarkerInput(Tensor<int> idx, Tensor<float> box)
        {
            var anchorPosition = DetectorInputSize * new float2(_assetLoader.Anchors[idx[0], 0], _assetLoader.Anchors[idx[0], 1]);

            var kp1 = MatrixUtils.Mul(M, anchorPosition + new float2(box[0, 0, 4 + (2 * 0) + 0], box[0, 0, 4 + (2 * 0) + 1]));
            var kp2 = MatrixUtils.Mul(M, anchorPosition + new float2(box[0, 0, 4 + (2 * 1) + 0], box[0, 0, 4 + (2 * 1) + 1]));
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
                Vector2 position = MatrixUtils.Mul(M2, new float2(landmarks[(5 * i) + 0], landmarks[(5 * i) + 1]));
                Vector3 word = (position - (_imageSource.Resolution / 2)) / _imageSource.Resolution.y;
                word.x += landmarks[(5 * i) + 2] / _imageSource.Resolution.y;

                Keypoints[i] = word;
            }
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
        }
    }
}