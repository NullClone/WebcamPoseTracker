using UnityEngine;

namespace WPT
{
    public sealed class InferenceRunnerDebugger : MonoBehaviour
    {
        // Fields

        [SerializeField] private Transform _parent;
        [SerializeField] private GameObject _prefab;

        private InferenceRunner _inferenceRunner;
        private GameObject[] _keypoints;


        // Methods

        private void Awake()
        {
            _inferenceRunner = gameObject.GetComponent<InferenceRunner>();
        }

        private void Update()
        {
            if (_inferenceRunner == null) return;

            if (_keypoints == null)
            {
                _keypoints = new GameObject[InferenceRunner.NumKeypoints];

                for (int i = 0; i < _keypoints.Length; i++)
                {
                    _keypoints[i] = Instantiate(_prefab, _parent);
                }
            }

            for (int i = 0; i < _keypoints.Length; i++)
            {
                _keypoints[i].transform.localPosition = _inferenceRunner.Keypoints[i];
            }
        }
    }
}