using Unity.InferenceEngine;
using UnityEngine;
using WPT.Utilities;

namespace WPT
{
    public sealed class ModelAssetLoader : MonoBehaviour
    {
        // Properties

        public Worker DetectorWorker => _detectorWorker;

        public Worker LandmarkerWorker => _landmarkerWorker;

        public float[,] Anchors { get; private set; }


        // Fields

        [SerializeField] private BackendType _backendType = BackendType.GPUCompute;
        [SerializeField] private ModelAsset _detector;
        [SerializeField] private ModelAsset _landmarker;
        [SerializeField] private TextAsset _anchors;

        private Model _detectorModel;
        private Model _landmarkerModel;
        private Worker _detectorWorker;
        private Worker _landmarkerWorker;


        // Methods

        private void Awake()
        {
            LoadModel();
            LoadWorker();

            Anchors = ModelUtils.LoadAnchors(_anchors.text);
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
                var detectorModel = ModelLoader.Load(_detector);
                var graph = new FunctionalGraph();
                var input = graph.AddInput(detectorModel, 0);
                var outputs = Functional.Forward(detectorModel, input);
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