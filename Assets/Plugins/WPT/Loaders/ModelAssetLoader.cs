using Unity.InferenceEngine;
using UnityEngine;
using WPT.Utilities;

namespace WPT
{
    public sealed class ModelAssetLoader : MonoBehaviour
    {
        // Fields

        [SerializeField] private BackendType _backendType = BackendType.GPUCompute;
        [SerializeField] private ModelAsset _detector;
        [SerializeField] private ModelAsset _landmarker;

        private Model _detectorModel;
        private Model _landmarkerModel;

        public Worker _detectorWorker;
        public Worker _landmarkerWorker;


        // Methods

        private void Start()
        {
            LoadModel();
            LoadWorker();
        }

        private void OnDestroy()
        {
            if (_detectorWorker != null)
            {
                _detectorWorker.Dispose();
                _detectorWorker = null;
            }

            if (_landmarkerWorker != null)
            {
                _landmarkerWorker.Dispose();
                _landmarkerWorker = null;
            }
        }

        private void LoadModel()
        {
            if (_detector)
            {
                _detectorModel = ModelLoader.Load(_detector);

                var graph = new FunctionalGraph();
                var input = graph.AddInput(_detectorModel, 0);
                var outputs = Functional.Forward(_detectorModel, input);
                var results = ModelUtils.ArgMaxFiltering(outputs[0], outputs[1]);

                _detectorModel = graph.Compile(results.Item1, results.Item2, results.Item3);
            }

            if (_landmarker)
            {
                _landmarkerModel = ModelLoader.Load(_landmarker);
            }
        }

        private void LoadWorker()
        {
            if (_detectorModel != null)
            {
                _detectorWorker = new Worker(_detectorModel, _backendType);
            }

            if (_landmarkerModel != null)
            {
                _landmarkerWorker = new Worker(_landmarkerModel, _backendType);
            }
        }
    }
}