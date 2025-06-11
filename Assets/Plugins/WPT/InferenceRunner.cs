using Unity.InferenceEngine;
using UnityEngine;

namespace WPT
{
    public sealed class InferenceRunner : MonoBehaviour
    {
        // Fields

        [SerializeField] private ModelAssetLoader _assetLoader;
        [SerializeField] private ImageSource _imageSource;

        private const int _numKeypoints = 33;
        private const int detectorInputSize = 224;
        private const int landmarkerInputSize = 256;


        // Methods

        private void Update()
        {
            if (_assetLoader == null || _imageSource == null) return;

            ExecuteModel();
        }

        private void ExecuteModel()
        {
            if (_imageSource.Texture == null ||
                _assetLoader._detectorWorker == null ||
                _assetLoader._landmarkerWorker == null) return;

            using var detectorInput = new Tensor<float>(new TensorShape(1, detectorInputSize, detectorInputSize, 3));
            using var landmarkerInput = new Tensor<float>(new TensorShape(1, landmarkerInputSize, landmarkerInputSize, 3));

            TextureConverter.ToTensor(_imageSource.Texture, detectorInput);
            TextureConverter.ToTensor(_imageSource.Texture, landmarkerInput);

            _assetLoader._detectorWorker.SetInput(0, detectorInput);
            _assetLoader._landmarkerWorker.SetInput(0, landmarkerInput);

            _assetLoader._detectorWorker.Schedule();
            _assetLoader._landmarkerWorker.Schedule();
        }
    }
}