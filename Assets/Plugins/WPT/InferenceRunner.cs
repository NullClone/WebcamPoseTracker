using System;
using Unity.InferenceEngine;
using UnityEngine;
using WPT.Utilities;

namespace WPT
{
    public sealed class InferenceRunner : MonoBehaviour
    {
        // Fields

        [SerializeField] private ModelAssetLoader _assetLoader;
        [SerializeField] private ImageSource _imageSource;

        private Tensor<float> _detectorInput;
        private Tensor<float> _landmarkerInput;
        private Awaitable _executeAwaitable;

        private const int _numKeypoints = 33;
        private const int detectorInputSize = 224;
        private const int landmarkerInputSize = 256;


        // Methods

        private async void Start()
        {
            if (_assetLoader == null || _imageSource == null) return;

            _detectorInput = new Tensor<float>(new TensorShape(1, detectorInputSize, detectorInputSize, 3));
            _landmarkerInput = new Tensor<float>(new TensorShape(1, landmarkerInputSize, landmarkerInputSize, 3));

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

        private async Awaitable ExecuteModel()
        {
            if (_assetLoader.DetectorWorker == null || _assetLoader.LandmarkerWorker == null) return;

            var size = Mathf.Max(_imageSource.Resolution.x, _imageSource.Resolution.y);

            var scale = size / (float)detectorInputSize;

            var M = MatrixUtils.Mul(
                MatrixUtils.TranslationMatrix(0.5f * (_imageSource.Resolution + new Vector2(-size, size))),
                MatrixUtils.ScaleMatrix(new Vector2(scale, -scale)));

            ImageUtils.SampleImageAffine(_imageSource.Texture, _detectorInput, M);

            _assetLoader.DetectorWorker.Schedule(_detectorInput);

            using var outputIdx = await (_assetLoader.DetectorWorker.PeekOutput(0) as Tensor<int>).ReadbackAndCloneAsync();
            using var outputScore = await (_assetLoader.DetectorWorker.PeekOutput(1) as Tensor<float>).ReadbackAndCloneAsync();
            using var outputBox = await (_assetLoader.DetectorWorker.PeekOutput(2) as Tensor<float>).ReadbackAndCloneAsync();

            if (outputScore[0] < 0.75f) return;

            Debug.Log(outputScore[0]);

            _assetLoader.LandmarkerWorker.Schedule(_landmarkerInput);

            using var landmarksAwaitable = await (_assetLoader.LandmarkerWorker.PeekOutput(0) as Tensor<float>).ReadbackAndCloneAsync();
        }
    }
}