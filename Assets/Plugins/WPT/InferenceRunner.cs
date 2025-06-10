using System.Collections;
using Unity.InferenceEngine;
using UnityEngine;

namespace WPT
{
    public sealed class InferenceRunner : MonoBehaviour
    {
        // Fields

        [SerializeField] private ModelAsset _modelAsset;
        [SerializeField] private BackendType _backendType = BackendType.GPUCompute;
        [SerializeField] private int _inputImageSize;
        [SerializeField] private ImageSource _imageSource;


        private Model _model;
        private Worker _worker;


        // Methods

        private void Start()
        {
            if (_modelAsset)
            {
                _model = ModelLoader.Load(_modelAsset);
            }

            if (_model == null) return;

            _worker = new Worker(_model, _backendType);
        }

        private void Update()
        {
            if (_worker == null) return;

            StartCoroutine(ExecuteModel());
        }

        private void OnDestroy()
        {
            if (_worker != null)
            {
                _worker.Dispose();
                _worker = null;
            }
        }


        private IEnumerator ExecuteModel()
        {
            if (_imageSource == null || _imageSource.Texture == null) yield break;

            var shape = new TensorShape(1, 3, _inputImageSize, _inputImageSize);

            var input = new Tensor<float>(shape);

            TextureConverter.ToTensor(_imageSource.Texture, input);

            yield return _worker.ScheduleIterable(input);

            input.Dispose();

            var scores = _worker.PeekOutput(0);
            var landmarks = _worker.PeekOutput(1);

            scores.Dispose();
            landmarks.Dispose();
        }
    }
}