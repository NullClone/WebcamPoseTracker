using UnityEngine;

namespace WPT
{
    public sealed class InferenceRunnerDebugger : MonoBehaviour
    {
        // Fields

        [SerializeField] private Keypoint[] _keypoints;

        private InferenceRunner _inferenceRunner;


        // Methods

        private void Awake()
        {
            _inferenceRunner = gameObject.GetComponent<InferenceRunner>();
        }

        private void Update()
        {
            if (_inferenceRunner == null || _keypoints == null) return;

            if (_keypoints.Length == InferenceRunner.NumKeypoints)
            {
                for (int i = 0; i < _keypoints.Length; i++)
                {
                    _keypoints[i].SetValue(
                        _inferenceRunner.Positions[i],
                        _inferenceRunner.Actives[i]);
                }
            }
        }
    }
}